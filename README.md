# Online Shop - Shopping Cart System

## Overview
This project is a client-server application developed in C# using .NET framework. The purpose of the application is to create and manage a shopping cart in an online store. The communication between the client and the server follows a client-server model, where the user interacts with the application through a simple user interface, allowing them to add, remove, or modify products in their shopping cart.

## User Features
- Adding products to the shopping cart
- Removing products from the shopping cart
- Modifying the quantity of products in the shopping cart
- Purchasing selected products

## Admin Features
- Displaying available products in the store
- Adding a new product to the store
- Removing a product from the store
- Displaying sales history in the store

## Communication and Data Storage
The application utilizes two processes for bidirectional client-server communication. The server process is responsible for processing data and requests sent by the client process. It handles operations such as data validation, verification, reading/writing data from/to the database (CSV files), and serialization/deserialization of objects.

The server process stores information about customers, products, and sales history. It also maintains logs of all transactions that occur in the application, including the date and details of each operation.

The client process sends requests to the server process to access and update the information stored on the server. For example, when a user wants to purchase selected products, they send a request to the server to update the product information in the store and record the transaction in the history.

Named pipes are used for bidirectional communication between the server and the client. The server process opens a NamedPipeServerStream, while the client process opens a NamedPipeClientStream. The Connect() and WaitForConnection() methods establish the connection between the two processes. StreamReader and StreamWriter objects are used for sending and receiving messages.

## Project Structure
The project is structured into two main parts:
- Client: Contains the client application code.
- Server: Contains the server application code.

### Client
- `Main()` method in the `Program` class initializes the communication with the server using named pipes.
- User options are presented in a loop, and the corresponding functionalities are implemented based on user input.
- The `ServerCommunication` class handles the communication with the server using the provided `StreamReader` and `StreamWriter` objects.

### Server
- `ShopApp` class is the entry point for the server application.
- The application loads customer data, product data, and shopping cart history data from CSV files using the `InputFileManager`.
- The server communicates with the client using named pipes and the `ClientCommunication` class, which utilizes the `StreamReader` and `StreamWriter` objects.
- The server handles operations such as login, registration, purchasing, modifying products, and more based on the received requests from the client.
- The `OutputFileManager` class is responsible for saving data to the CSV files.

## Usage
1. Clone the repository and open the project in Visual Studio.
2. Build and run the server application.
3. Build and run the client application.
4. The client application will prompt the user with options to login, register, or exit.
5. Depending on the selected option, the user will either log in as a customer or as an admin.
6. If the user logs in as an admin, they will have access to the admin panel with additional functionalities.
7. If the user logs in as a customer, they can add, remove, and modify products in their shopping cart, as well as make purchases.
8. The server application will handle the requests from the client, update the database, and send back the necessary data.
