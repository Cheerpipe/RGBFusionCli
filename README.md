# RGBFusionBridge

## Descripción

RGBFusionBridge es una herramienta muy simple que sirve para cambiar el color, modo, velocidad y brillo de dispositivos compatibles con [Gigabyte's RGB Fusion]
Para funcionar, `RGBFusionBridge` carga e inicializa los componentes internos de [Gigabyte's RGB Fusion] para utilizarlos a modo de HAL debido a que [Gigabyte's RGB Fusion] es capaz de manipular hardware de distintos tipos y marcas.
Las tareas de inicialización pueden tardar varios segundos. Para evitar que esta demora ocurra cada vez que se aplica un cambio en la iluminación, `RGBFusionBridge` inicia sus componentes internos y luego se queda a la espera de comandos enviados por linea de comandos o por un NamedPipe.

## Modos de uso 

Esta utilidad puede ser utilizada de dos maneras distintas:

1. Línea de comandos
2. Enviando mensajes a un NamedPipe. Este modo fue desarrollado para ser usado con aplicaciones como `Aurora Project` u otras que requieran del envío de comandos rápidamente (hasta 15 comandos por segundo).

## Características

* Bajo uso de CPU.
* Hasta 15 comandos por segundo usando NamedPipe.

## Dependencias

Esta aplicación depende de `LedLib2.dll` y `SelLEDControl.dll`, ambos parte de la aplicación principal de [Gigabyte's RGB Fusion]. Ambas son necesarias para su compilación.
	
## Instalación

Esta aplicación no necesita ser instalada. Solo debes copiar `RGBFusionBridge.exe` en la carpeta de [Gigabyte's RGB Fusion Application]. Típicamente `C:\Program Files (x86)\GIGABYTE\RGBFusion`.



### Comandos

Obtener el listado de identificadores de áreas.

```
--areas
```

Establecer áreas

```
--setarea:<AREA_ID>:<MODE_ID>:<R_VALUE>:<B_VALUE>:<G_VALUE>:<SPEED_VALUE>:<BRIGHT_VALUE>
```

Donde:

- <AREA_ID>: Número de área. Los identificadores de áreas se pueden obtener con el comando `--areas` o usar `-1` para establecer todas las áreas. Si se usa el área -1, el parámetro `<MODE_ID>` será ignorado.
- <MODE_ID>: Valor numérico que establece el modo de operación del área. Ver  **Identificadores de modo**
- <R_VALUE>: Valor entre 0 y 255 que representa el color rojo.
- <G_VALUE>: Valor entre 0 y 255 que representa el color verde.
- <B_VALUE>: Valor entre 0 y 255 que representa el color azul.
- <SPEED_VALUE>: OPCIONAL (Valor predeterminado es 5) Valor entre 0 y 9 que representa la velocidad de la animación si se usa un modo animado.
- <BRIGHT_VALUE>: OPCIONAL (Valor predeterminado es 9) Valor entre 0 y 9 que representa el nivel de brillo del área. 

```
--loadprofile: <PROFILE_ID>
```
Donde:

- <PROFILE_ID>: Número de perfil de  [Gigabyte's RGB Fusion] que se desea cargar.

 **Identificadores de modo:**
- Still = 0;
- Breath = 1;
- Beat = 2;
- MixColor = 3;
- Flash = 4;
- Random = 5;
- Wave = 6;
- Scenes = 7;
- off = 8;
- auto = 9;
- other = 10;
- DFlash = 11;


## Transacciones

RGBFusionBridge permite enviar la ejecución de comandos enviados de manera secuencial somo si fuese un solo comando a través del uso de transacciones. Para esto es necesario ejecutar la siguiente secuencia de comandos:

```
--transactionstart

<SET AREAS COMMANDS>

--transactioncommit
```

El comando `--transactionstart` iniciará el modo transaccional, luego de esto, todos los comandos que se envien serán encolados y ejecutados como si fuese un comando único al ejecutar el comando `--transactioncommit`.

Ejemplo:

```
--transactionstart        	<- Inicia el modo transacción
--setarea:0:0:255:0:0     	<- Crea un comando de cambio de color y lo encola.
--setarea:2:0:0:255:0		<- Crea un comando de cambio de color y lo encola.
--setarea:4:0:0:0:255		<- Crea un comando de cambio de color y lo encola.
--setarea:6:0:0:255:255		<- Crea un comando de cambio de color y lo encola.
--transactioncommit		<- Aplica todos los comandos enviados desde el inicio de la transacción y termina el modo trandacción.
```

Es importante destacar que no todas las áreas son compatibles con todos los modos. Tendrás que probar y ver que modos funcionan para cada área.
