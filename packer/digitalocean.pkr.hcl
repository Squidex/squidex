variable "squidex_version" {
  type = string
  default = "5.8.0"
}

source "digitalocean" "squidex" {
  image = "ubuntu-20-04-x64"
  region = "sfo3"
  droplet_name = "squidex-${replace(var.squidex_version, ".", "-")}-build-{{ timestamp }}"
  snapshot_name = "squidex-${replace(var.squidex_version, ".", "-")}-{{ timestamp }}"
  snapshot_regions = [
    "nyc1",
    "sfo1",
    "nyc2",
    "ams2",
    "sgp1",
    "lon1",
    "nyc3",
    "ams3",
    "fra1",
    "tor1",
    "sfo2",
    "blr1",
    "sfo3",
  ]
  size = "s-2vcpu-2gb"
  ssh_username = "root"
}

build {
  sources = [
    "source.digitalocean.squidex"
  ]

  provisioner "ansible" {
    ansible_env_vars = [
      "ANSIBLE_HOST_KEY_CHECKING=False",
      "ANSIBLE_SSH_ARGS='-F /dev/null -o ForwardAgent=no -o ControlMaster=auto -o ControlPersist=60s'",
      "ANSIBLE_NOCOLOR=True"
    ]
    extra_arguments = ["--extra-vars", "squidex_version=${var.squidex_version}"]
    playbook_file = "./ansible/playbook.yml"
    use_proxy = false
  }
}