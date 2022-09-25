NAME=schemaapi
PLATFORM=linux/amd64
CONTAINER_REGISTRY=homepi:14000
BUILD_NUMBER=latest
OUTPUT_DIR=${PWD}/.output
OUTPUT_DIR_MANIFESTS=$(OUTPUT_DIR)/manifests
CONFIGURATION=Debug

init: clean restore build app

clean:
	@-rm -Rf $(OUTPUT_DIR)
	@mkdir $(OUTPUT_DIR)
	@mkdir $(OUTPUT_DIR_MANIFESTS)

restore:
	@cd src && dotnet restore -v q

build:
	@cd src && dotnet build -c $(CONFIGURATION) -v q

app:
	@cd src/SchemaApi.App && dotnet publish \
		--configuration $(CONFIGURATION) \
		--no-build \
		-v q \
		-o $(OUTPUT_DIR)/app \
		SchemaApi.App.csproj

container:
	docker buildx build --platform $(PLATFORM) -t $(NAME) . --load

deliver:
	docker tag $(NAME):latest $(CONTAINER_REGISTRY)/$(NAME):$(BUILD_NUMBER)
	docker push $(CONTAINER_REGISTRY)/$(NAME):$(BUILD_NUMBER)

manifests:
	@cp -r ./k3s-manifests.yml $(OUTPUT_DIR_MANIFESTS)/
	@find "$(OUTPUT_DIR_MANIFESTS)" -type f -name '*.yml' | xargs sed -i '' -e 's:{{BUILD_NUMBER}}:${BUILD_NUMBER}:g'

ci: clean restore build app container manifests

deploy:
	cd $(OUTPUT_DIR_MANIFESTS) && kubectl apply -f .

cd: CONFIGURATION=Release
cd: PLATFORM=linux/arm64
cd: BUILD_NUMBER=$(shell date '+%Y%m%d%H%M%S')
cd: ci deliver deploy
	cd $(OUTPUT_DIR_MANIFESTS) && kubectl apply -f .

dev:
	cd src && dotnet watch --no-hot-reload --project SchemaApi.App/ run