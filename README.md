# Doviz Kurlari ASP.NET Project
 
![UygulamaGörüntüleri-1](https://user-images.githubusercontent.com/43846785/140271806-f0b7e49c-3db7-4ac7-bfd9-93636a555e66.png)


Currency Rates app created with JavaScript,C# and MSSQL Server.
## Table of contents
* [General Info](#general-info)
* [Technologies](#technologies)
* [Database Connection](#database-connection)
* [Code Examples](#code-examples)
* [Setup](#setup)
* [ScreenShots](#screenshots)

## General Info
* This project provides the display of Currency Rates with the help of graphics.
* It saves Dollar, Euro and Sterling data in the database by dynamically pulling from https://www.tcmb.gov.tr/kurlar/today.xml link.
* When the application is not entered for a certain day, the data is retrieved and saved in the database by going backwards from the day the user entered the application. 
* The project was created to be used by the company where I worked as an intern.
	
 ## Technologies
Project is created with:
* ASP.NET MVC 5
* Entity Framework
* JavaScript
* HTML5
* CSS
* Bootstrap 4

## Database Connection
* In this project, Entity Framework Database First method is used for the database. The DovizEntities model has been added to the Model class using Entity.A single table was created in the database and Dollar, Euro and Sterling data were added to the table.
* The view of the created table should be as follows:
* ![Veritabanı Tablo Görünümü](https://user-images.githubusercontent.com/43846785/140274355-db78f7a0-653f-482b-b607-e9b3d229b615.png)
* After the table is created, it should be added to the project with the Entity Model.
* If there is more than one table in your database, it will be sufficient to add only the table to be used in this project. Its appearance is as follows:
* ![Tablo Proje Bağlantısı](https://user-images.githubusercontent.com/43846785/140274781-a947536c-47f2-4af5-8275-431ecdcab72a.png)

## Code Examples
 
* A variable has been created in order to dynamically pull the data shared by the CBRT:`$"https://www.tcmb.gov.tr/kurlar/2021{Month}/{NewDate}2021.xml";`
* Day and month variables are created so that the data can be pulled dynamically as it is shared:`NewDate = CurrentDate.ToString("ddMM");`,`Month = CurrentDate.ToString("MM");`
* Saving the captured data to the database is as follows:
* ![verilerinVeritabanınaKaydedilmesi](https://user-images.githubusercontent.com/43846785/140273778-04c76c9c-6e45-4e96-9694-b99baef44b06.png)
	
## Setup
* The project is started by clicking the .sln file.
* In the project, 6 months of Dollar, Euro and Sterling data are dynamically captured and saved in the database. Therefore, the SaveSixMonthData function should be run once in the Index section and commented out: `SaveSixMonthData()`
* After this process, the project is ready to run.
## ScreenShots
* Some screenshots from the project:
* ![3AylıkKarışıkData](https://user-images.githubusercontent.com/43846785/140275286-295998e9-50fa-41f0-ad5d-3037a6568a97.png)
* ![eurUSDGrafik](https://user-images.githubusercontent.com/43846785/140275318-c23aefc3-2053-4a6e-9c40-12be9ac50307.png)
* ![UygulamaGörüntüleri-2](https://user-images.githubusercontent.com/43846785/140275946-0b2b9b9f-7659-41a7-8047-b1d773049f5f.png)


