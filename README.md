# Moonbeam
A MBM&lt;->XML converter with a Spanish dictionary

Minimum requisites
-----------------------------------------------------------
.NET Framework 4.7

Spanish
-----------------------------------------------------------
Abre una consola y aplica los parametros correspondientes junto a la ruta del archivo.
Por ejemplo:
"Moonbeam.exe -e "C:\carpeta""
El programa tiene varios parametros y modos de uso.
Los primeros dos parametros que hay que insertar son los siguientes:

	- "-e" para exportar de MBM a XML
	- "-i" para importar de XML a MBM
	- "-v" para exportar de (n ->1) MBM a XML, es decir, de varios MBM un solo XML (No hecho para traducir)
	
Tras esto, hay dos formas de usar el programa: el modo explicito y el modo recursivo.

El modo explicito se activa al introducir en la ruta directamente el archivo a convertir y solo tratará dicho archivo.
Por ejemplo:
"Moonbeam.exe -e "C:\carpeta\archivo.mbm""

El modo recursivo se activa al introducir en la ruta un directorio, dejando implicito que dentro de ese directorio hay archivos,
la busqueda recursiva tratará de buscar todos los archivos a convertir dentro de una ruta.
Por ejemplo:
"Moonbeam.exe -e "C:\carpeta\"

English
-----------------------------------------------------------
Open a CLI and apply the arguments next to the file's path
For example:
"Moonbeam.exe -e "C:\folder""
The program has various parameters and use modes.
The first two parameters that you need to insert are:

	- "-e" to export MBM to XML
	- "-i" to import XML to MBM
	- "-v" to export a (n ->1) MBM to XML, from n MBM 1 XML, not for translation purposes
	
Then, there's two use modes: explicit and recursive mode.

The explicit mode is enabled when you apply into the path the file to convert, it will only convert that file.
For example:
"Moonbeam.exe -e "C:\folder\file""

The recursive mode is enable when you only apply into the path the folder where you imply there are files, the recursive
search will try to find all the files to convert inside the path.
For example:
"Moonbeam.exe -e "C:\folder""
