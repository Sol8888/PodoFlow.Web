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

    const contenido = await referencia.arrayBuffer();
    const escritor = await destino.createWritable();

    await escritor.write(contenido);
    await escritor.close();
}

export async function descargarArchivo(nombre, tipo, referencia) {
    const contenido = await referencia.arrayBuffer();
    const archivo = new Blob([contenido], { type: tipo });
    const url = URL.createObjectURL(archivo);
    const enlace = document.createElement("a");

    enlace.href = url;
    enlace.download = nombre;
    document.body.appendChild(enlace);
    enlace.click();
    enlace.remove();
    URL.revokeObjectURL(url);
}

export async function copiarTexto(texto) {
    try {
        await navigator.clipboard.writeText(texto);
        return true;
    } catch {
        return false;
    }
}
