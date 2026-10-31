let destinoElegido = null;

export function admiteGuardarComo() {
    return typeof window.showSaveFilePicker === "function";
}

export async function elegirDestino(nombre, tipo) {
    const punto = nombre.lastIndexOf(".");
    const extension = punto >= 0 ? nombre.substring(punto) : "";
    const opciones = { suggestedName: nombre };

    if (extension) {
        opciones.types = [{ description: "Exportación de PodoFlow", accept: { [tipo]: [extension] } }];
    }

    destinoElegido = null;

    try {
        destinoElegido = await window.showSaveFilePicker(opciones);
        return "elegido";
    } catch (error) {
        if (error && error.name === "AbortError") {
            return "cancelado";
        }

        throw error;
    }
}

export async function escribirArchivo(referencia) {
    const destino = destinoElegido;
    destinoElegido = null;

    if (!destino) {
        throw new Error("No hay una ubicación elegida para guardar el archivo.");
    }

    const contenido = await leerContenido(referencia);
    const escritor = await destino.createWritable();

    try {
        await escritor.write(contenido);
        await escritor.close();
    } catch (error) {
        await escritor.abort().catch(() => { });
        throw error;
    }

    const guardado = await destino.getFile();

    if (guardado.size !== contenido.byteLength) {
        throw new Error("El archivo guardado no quedó completo.");
    }
}

export async function descargarArchivo(nombre, tipo, referencia) {
    const contenido = await leerContenido(referencia);
    const archivo = new Blob([contenido], { type: tipo });
    const url = URL.createObjectURL(archivo);
    const enlace = document.createElement("a");

    enlace.href = url;
    enlace.download = nombre;
    enlace.style.display = "none";
    document.body.appendChild(enlace);
    enlace.click();

    setTimeout(() => {
        enlace.remove();
        URL.revokeObjectURL(url);
    }, 60000);
}

async function leerContenido(referencia) {
    const contenido = await referencia.arrayBuffer();

    if (!contenido || contenido.byteLength === 0) {
        throw new Error("El archivo recibido está vacío.");
    }

    return contenido;
}

export async function copiarTexto(texto) {
    try {
        await navigator.clipboard.writeText(texto);
        return true;
    } catch {
        return false;
    }
}
