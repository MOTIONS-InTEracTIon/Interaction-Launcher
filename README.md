# Interaction Launcher
 Launcher for experiences 

Videos:

Demo de funciones principales: https://drive.google.com/file/d/1ALqRAxy2odvdaHaqom1MyCYqDC809scy/view?usp=sharing
Descripción de sistemas a fondo: https://drive.google.com/file/d/1TkDiSc1ARMMK0gIe5DwCvsEpcvYUsMs0/view?usp=sharing
 
Useful Info:

Para inicializar una aplicación o lanzar un nuevo release (Para aplicaciones de Unity, se puede usar como referencia):
- Agregar addons a una carpeta con el nombre del tipo del addon en root/Addons/
     Ej. root/Addons/Environment/
- Si se tiene addons.json en root, borrarlo.
- Se generará un nuevo addons.json que eventualmente interpretará el Launcher- (Ver ejemplo de generacion en AddonsController con los addons tipo Environment)
- El addon puede constar de uno o más .bundle, estos deben ser todos comprimidos en zip y subidos en el release de github-
- Eliminar .bundles de la carpeta root/Addons- (Reduciendo considerablemente el peso de la aplicación)
- Generar .zip de toda la aplicación con el nombre del github tag.
    Ej tag: v1.5.0 archivo: v1.5.0.zip
- Luego de estas acciones, la aplicación esta lista para ser recibida por el Launcher con Input Binding y Addons.
- Agregar información como repositorio y urls de imágenes a settings.json de Interaction Launcher
- Lanzar un nuevo release con este settings.json si se quiere añadir la aplicación a futuras descargas del Launcher.

Consideración: Para usar el sistema de localización del launcher, debe haber almenos un archivo locale-<lang>.json en el ultimo commit de github de la aplicación en la carpeta Launcher/Localization, ver ejemplo locale-en.json tanto en Interaction Launcher como en VoRTIcES 2.0.

Nuevos idiomas en launcher:
- Crear nuevo archivo json locale utilizando alguno de ejemplo en la carpeta Interaction Launcher_Data/StreamingAssets/baseStrings.
