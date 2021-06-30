variable "squidex_version" {
  type = string
  default = "5.7.0"
}

source "amazon-ebs" "squidex" {
  ami_groups = [
    "all"]
  ami_virtualization_type = "hvm"
  ami_name = "squidex-${replace(var.squidex_version, ".", "-")}-{{ timestamp }}"
  instance_type = "t2.small"
  region = "us-east-1"
  source_ami_filter {
    filters = {
      virtualization-type = "hvm"
      name = "ubuntu/images/*/ubuntu-focal-20.04-amd64-server-*"
      root-device-type = "ebs"
    }
    owners = [
      "099720109477"]
    most_recent = true
  }
  ssh_username = "ubuntu"
  ami_regions = [
    "eu-north-1",
    "ap-south-1",
    "eu-west-3",
    "eu-west-2",
    "eu-west-1",
    "ap-northeast-3",
    "ap-northeast-2",
    "ap-northeast-1",
    "sa-east-1",
    "ca-central-1",
    "ap-southeast-1",
    "ap-southeast-2",
    "eu-central-1",
    "us-east-1",
    "us-east-2",
    "us-west-1",
    "us-west-2",
  ]
}

build {
  sources = [
    "source.amazon-ebs.squidex"
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