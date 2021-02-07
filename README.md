# Citizerve
Citizerve has been created to demonstrate Cloud technologies. 
The story is that Citizerve is a multi-tenant cloud offering that government customers use to serve their citizens. Customers can add all their citizens to Citizerve and provision resources (like COVID vaccine) for them.

![alt text](https://github.com/dushyantgill/citizerve/blob/main/res/main-branch-architecture.png?raw=true)

## Citizerve microservices
Citizerve consists of 2 .Net Core Web APIs (CitizenAPI and ProvisionAPI) and 2 .Net Core Worker Services (SyncWorker and ProvisionWorker)
- **CitizenAPI** web api depends on/calls ProvisionAPI. And CitizenAPI CRUDs citizens in an Azure Cosmos DB (Mongo API).
- **ProvisionAPI** web api CRUDs resources in the Azure Cosmos DB (Mongo API) and writes resource messages to two Azure Service Bus queues.  
- **ProvisionWorker** worker service reads resource messages from the provision queue and processes each message for 30-60 seconds.
- **SyncWorker** worker service generates load on CitizenAPI (and thus the entire system) by creating 10 citizens every 15 seconds, and executing 100 searches every 5 seconds, and deleting 10 citizens every minute. SyncWorker reads name and address data from an Azure SQL DB.

The web APIs use Azure Active Directory for authentication and authorization.
And the microservices read connection strings and secrets for CosmosDB, ServiceBus, and SQLDB from an Azure KeyVault. The KeyVault holds the following secrets:

## Download and run your own Citizerve
First, create the necessary resources in your Azure. Then, download the Citizerve code to run on your dev machine with Visual Studio 2019.

### Create Azure Resources

#### 1. Create Azure Active Directory Applications
Create an Azure Active Directory tenant and register two applications in that tenant.

- **Citizerve**: this application represents the APIs of Citizerve: CitizenAPI and ProvisionAPI. Create 4 app roles for Citizerve app: *Global.Citizens.Read*, *Global.Citizens.Write*, *Global.Resources.Read*, and *Global.Resources.Write*

Here's the Azure AD application manifest file that you can customize for your Citizerve app creation: [**Citizerve-AzureAD-ApplicationManifest**](https://github.com/dushyantgill/citizerve/blob/main/deploy/azuread/Citizerve-AzureAD-ApplicationManifest.json)

- **Citizerve Sync**: this application represents the SyncWorker worker service. Create a client secret for this app. And add the 4 app roles of Citizerve application to the required API permissions for the Citizerve Sync app.

Here's the Azure AD application manifest file that you can customize for your Citizerve Sync app creation: [**Citizerve Sync-AzureAD-ApplicationManifest**](https://github.com/dushyantgill/citizerve/blob/main/deploy/azuread/Citizerve%20Sync-AzureAD-ApplicationManifest.json)

#### 2. Create Azure CosmosDB
Create an CosmosDB Account with *MondoDB API*. Create a database called CitizenDb and in that database create two collections
- **Citizens** collection with shard key *citizenId*
- **Resources** collection with shard key *resourceId*

Here's the Azure Resource Manager template that you can customize for your deployment: [**Create CosmosDB template**](https://github.com/dushyantgill/citizerve/blob/main/deploy/azure/create-cosmosdb-template.json)

#### 3. Create Azure Service Bus
Create a Service Bus Namespace with two queues *provision* and *deprovision*. The ProvisionAPI web API will write to these queues. And the ProvisionWorker service will read from the queue.

Here's the Azure Resource Manager template that you can customize for your deployment: [**Create ServiceBus template**](https://github.com/dushyantgill/citizerve/blob/main/deploy/azure/create-servicebus-template.json)

#### 4. Create Azure SQL DB
Create a SQL database to store fake names and address data. The SyncWorker worker service will read this database and create fake citizens. 

Here's the Azure Resource Manager template that you can customize for your deployment: [**Create SQLDB template**](https://github.com/dushyantgill/citizerve/blob/main/deploy/azure/create-sqldb-template.json)

Then, create 5 tables and import the fake data using the following schema and data insert scripts 
- **GivenNames**: use this script to create the table and insert 5101 fake given names: [**dbo.GivenNames.Table.sql**](https://github.com/dushyantgill/citizerve/blob/main/scripts/sql/dbo.GivenNames.Table.sql)
- **Surnames**: use this script to create the table and insert 18818 fake surnames: [**dbo.Surnames.Table.sql**](https://github.com/dushyantgill/citizerve/blob/main/scripts/sql/dbo.Surnames.Table.sql)
- **StreetNames**: use this script to create the table and insert 1256 fake street names: [**dbo.Surnames.Table.sql**](https://github.com/dushyantgill/citizerve/blob/main/scripts/sql/dbo.StreetNames.Table.sql)
- **Cities**: use this script to create the table and insert 23400 fake city records: [**dbo.Surnames.Table.sql**](https://github.com/dushyantgill/citizerve/blob/main/scripts/sql/dbo.Cities.Table.sql)
- **Customers**: use this script to create the table: [**dbo.Customers.Table.sql**](https://github.com/dushyantgill/citizerve/blob/main/scripts/sql/dbo.Customers.Table.sql). Then, insert a record for your Azure AD tenant. 

Your SQL DB should look like this:

![alt text](https://github.com/dushyantgill/citizerve/blob/main/res/sqldb-fakedata-aftersetup.png?raw=true)

#### 5. Create Azure Key Vault
Create a Key Vault to store the connection strings of your Azure resources and the client secret of your Azure AD application.

Then, create 4 secrets in the Key Vault:
- **citizerve-syncworker-azuread-clientsecret-primary**: the password credential of the Azure AD Citiserve Sync application that SyncWorker microservice uses to authenticate to the CitizenAPI 
- **citizerve-cosmosdb-connectionstring-primary**: the connection string for Citizerve Cosmos DB that CitizenAPI and ProvisionAPI use to CRUD citizens and resources.
- **citizerve-servicebus-connectionstring-primary**: the connection string for Citizerve ServiceBus that ProvisionAPI uses to write to queue and ProvisionWorker uses to read from queue.
- **citizerve-sqldb-connectionstring-primary**: the connection string for Citizerve SQLDB that SyncWorker uses to read fake names and address data.

Here's the Azure Resource Manager template that you can customize for your deployment: [**Create KeyVault template**](https://github.com/dushyantgill/citizerve/blob/main/deploy/azure/create-keyvault-template.json)

### Download Citizerve code and configure 
Download the Citizerve code from the desired branch and open in Visual Studio 2019

**main** branch is just the microservices code. No packaging or deployment code. No monitoring/instrument/logging code.

#### Update appsettings.json files
1. Update *AzureAd-Domain*, *AzureAd-ClientId*, and *KeyVaultEndpoint* in appsettings.json of CitizenAPI project
2. Update *AzureAd-Domain*, *AzureAd-ClientId*, and *KeyVaultEndpoint* in appsettings.json of ProvisionAPI project
3. Update *KeyVaultEndpoint* in appsettings.json of ProvisionWorker project
5. Update *AzureAd-ClientId*, *AzureAd-Resource*, and *KeyVaultEndpoint* in appsettings.json of SyncWorker project

#### Signin to Visual Studio and grant access to Key Vault
Signin to Visual Studio 2019 using your developer user account and grant the same user account secret read permissions on your Azure Key Vault.

>Finally, set all 4 projects as startup projects in Visual Studio and hit F5. Enjoy!