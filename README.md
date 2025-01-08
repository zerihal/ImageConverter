# ImageConverter

Image rescaling and conversion service that converts IFormFile image file to new size by width and height or percentage with optional change of file format (e.g. .jpg to .png), or simply converts the file format.

API functions (Swagger):

![image](https://github.com/user-attachments/assets/3c8d6075-00cf-463f-87d2-ba7e28c2ed2a)

Supported image formats for upload and conversion are jpg, png, bmp, tiff, and gif. When changing the extension this should be specified as ".[extension]", for example ".jpg", ".png", etc.

Can be deployed as a standalone API or containerised microservice.
