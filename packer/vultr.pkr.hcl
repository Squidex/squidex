packer {
  required_plugins {
    vultr = {
      version = ">= 2.3.1"
      source = "github.com/vultr/vultr"
    }
  }
}

variable "squidex_version" {
  type = string
  default = "5.8.0"
}

variable "vultr_api_key" {
  type    = string
  default = "${env("VULTR_API_KEY")}"
}

source "vultr" "squidex" {
  api_key = "${var.vultr_api_key}"
  region_id = "lax"
  plan_id = "vc2-1c-2gb"
  os_id = "387" // Ubuntu 20.04
  hostname = "squidex-${replace(var.squidex_version, ".", "-")}-build-{{ timestamp }}"
  snapshot_description = "squidex-${replace(var.squidex_version, ".", "-")}-{{ timestamp }}"
  ssh_username = "root"
  state_timeout = "10m"
}

build {
  sources = [
    "source.vultr.squidex"
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