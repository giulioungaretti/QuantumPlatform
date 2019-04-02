client:
	docker build  -f ./Client/Dockerfile .  --tag=client

silo:
	docker build -f ./Silo.Host/Dockerfile . --tag=silo


run: silo  client
	docker-compose up
