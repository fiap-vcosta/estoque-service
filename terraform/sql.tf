locals {
  db_instance_name = data.terraform_remote_state.infra_db.outputs.instance_name
  db_name          = "estoque"
  db_user          = "estoque"
  db_role          = "estoque_app"
  provisioner_user = "estoque_provisioner"
}

resource "random_password" "provisioner" {
  length  = 32
  special = false
}

resource "random_password" "app" {
  length  = 32
  special = false
}

resource "google_sql_database" "service" {
  name     = local.db_name
  instance = local.db_instance_name
  project  = var.project_id
}

resource "google_sql_user" "provisioner" {
  name     = local.provisioner_user
  instance = local.db_instance_name
  project  = var.project_id
  password = random_password.provisioner.result
}

resource "google_secret_manager_regional_secret" "provisioner_password" {
  secret_id = "estoque-provisioner-password"
  location  = var.region
  project   = var.project_id

  labels = {
    service = "estoque-service"
    purpose = "data-api"
  }
}

resource "google_secret_manager_regional_secret_version" "provisioner_password" {
  secret      = google_secret_manager_regional_secret.provisioner_password.id
  secret_data = random_password.provisioner.result
}

resource "google_sql_provision_script" "harden" {
  instance    = local.db_instance_name
  database    = google_sql_database.service.name
  description = "Role estoque_app + REVOKE CONNECT PUBLIC"
  script      = file("${path.module}/harden.sql")
  user        = google_sql_user.provisioner.name

  password_secret_version = google_secret_manager_regional_secret_version.provisioner_password.name

  depends_on = [
    google_sql_user.provisioner,
    google_secret_manager_regional_secret_version.provisioner_password,
  ]
}

resource "google_sql_user" "app" {
  name           = local.db_user
  instance       = local.db_instance_name
  project        = var.project_id
  password       = random_password.app.result
  database_roles = [local.db_role]

  depends_on = [google_sql_provision_script.harden]
}

resource "terraform_data" "drop_database_before_app_user" {
  input = {
    project  = var.project_id
    instance = local.db_instance_name
    database = local.db_name
  }

  depends_on = [
    google_sql_database.service,
    google_sql_user.app,
  ]

  provisioner "local-exec" {
    when        = destroy
    interpreter = ["/bin/bash", "-c"]
    command     = <<-EOT
      set -euo pipefail
      gcloud sql databases delete '${self.input.database}' \
        --instance='${self.input.instance}' \
        --project='${self.input.project}' \
        --quiet
    EOT
  }
}
