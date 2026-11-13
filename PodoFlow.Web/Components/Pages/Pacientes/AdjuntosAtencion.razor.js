export async function descargarArchivo(nombre, tipo, referencia) {
    const contenido = await referencia.arrayBuffer();
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
