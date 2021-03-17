MODULE := "homer-squidex"
PROJECT_DIR := $(dir $(abspath $(lastword $(MAKEFILE_LIST))))
STAGE ?= "dev"
DOCKER_SERVER ?= 597764168253.dkr.ecr.us-east-1.amazonaws.com
DOCKER_IMAGE ?= ${DOCKER_SERVER}/homer-squidex
VERSION ?= $(shell git branch --show-current)-$(shell git rev-parse --short HEAD)

ifndef AWS_PROFILE
$(error AWS_PROFILE is not set)
endif

# Get account ID from AWS Account Id)
# AWS_ACCOUNT_ID := $(shell $(SCRIPTS_DIR)/get_aws_account_id.sh)
# TERRAFORM_WORKSPACE ?= "$(MODULE)-$(STAGE)-$(AWS_ACCOUNT_ID)"

# lists all available targets
list:
	@sh -c "$(MAKE) -p no_targets__ | \
		awk -F':' '/^[a-zA-Z0-9][^\$$#\/\\t=]*:([^=]|$$)/ {split(\$$1,A,/ /);\
		for(i in A)print A[i]}' | \
		grep -v '__\$$' | \
		grep -v 'make\[1\]' | \
		grep -v 'Makefile' | \
		sort"

# required for list
no_targets__:

build: info
	docker build . -t "${DOCKER_IMAGE}"
# 	docker image prune -f

deploy: build
	aws ecr get-login-password --region us-east-1 | docker login --username AWS --password-stdin ${DOCKER_SERVER}
	docker tag ${DOCKER_IMAGE}:latest ${DOCKER_IMAGE}:${VERSION}
	docker push ${DOCKER_IMAGE}:latest
	docker push ${DOCKER_IMAGE}:${VERSION}


_login:
	AWS_PROFILE=homer-prod aws ecr get-login-password --region us-east-1 | docker login --username AWS --password-stdin ${DOCKER_SERVER}

info:
	@echo "   project dir: $(PROJECT_DIR)"
	@echo "         stage: $(STAGE)"
	@echo "   aws profile: $(AWS_PROFILE)"
	@echo "   docker image: $(DOCKER_IMAGE)"
	@echo "   version: $(VERSION)"
	@echo