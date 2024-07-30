# Document Mangament System

## Description
This project is an in house soulution for organizing documents and tracking changes and comments .

## Table of Contents
- [Installation](#installation)
- [Usage](#usage)
- [Features](#features)
- [License](#license)

## Installation
1. Clone the repository: `git clone https://github.com/YousefHSS/DocManagmentSystem.git`.
2. make sure required nuget packages are installed in using Package Manager Console.
3. intall SQL server and copy the conncetion string to the appsettings.json
4. change the email and the password to your desired email and password in appsettings.json (this will be the sender email for the app).
5. download libre office portable and change the LibreOfficePath variable in appsettings.json

## Usage
- After installing you can run or deploy the app using Visual Studio 

## Features
- Uploading documents to a pipeline where they can be approved or rejected
- Archving older versions of the documents.
- An Audit log for actions done on document when they are taken and by whom.
- Preview Document for quick rejections or comments.
- Notifaction system using email for the concerned parties of the documents status.
- Adding Comments to Document on the fly inside the app (in progress).
- Adding new documents using templates (in progess).

## Contributors

- [Yousef Hussein Salem](https://github.com/YousefHSS)(https://github.com/yousefhussen)


## License

This project is licensed under the [MIT License](LICENSE).

