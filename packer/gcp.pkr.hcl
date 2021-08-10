variable "squidex_version" {
  type = string
  default = "5.8.0"
}

source "googlecompute" "squidex" {
  project_id = "squidex-315304"
  ssh_username = "root"
  zone = "us-central1-a"
  source_image_family = "ubuntu-2004-lts"
  instance_name = "squidex-${replace(var.squidex_version, ".", "-")}-build-{{ timestamp }}"
  image_name = "squidex-${replace(var.squidex_version, ".", "-")}-{{ timestamp }}"
  image_family = "squidex"
  image_storage_locations = [
    "us"]
}

build {
  sources = [
    "source.googlecompute.squidex"
  ]

  provisioner "ansible" {
    ansible_env_vars = [
      "ANSIBLE_HOST_KEY_CHECKING=False",
      "ANSIBLE_SSH_ARGS='-F /dev/null -o ForwardAgent=no -o ControlMaster=auto -o ControlPersist=60s'",
      "ANSIBLE_NOCOLOR=True"
    ]
    extra_arguments = [
      "--extra-vars",
      "squidex_version=${var.squidex_version}"]
    playbook_file = "./ansible/playbook.yml"
    use_proxy = false
  }
}