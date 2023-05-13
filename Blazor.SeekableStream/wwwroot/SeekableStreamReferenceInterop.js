export async function createStreamReference(stream) {
    if (stream instanceof Blob || stream instanceof ArrayBuffer || ArrayBuffer.isView(stream)) {
        // Do nothing
    } else if (stream instanceof Response) {
        stream = await stream.blob();
    } else {
        throw new Error("Invalid Stream input. This method accepts Blob, Response, ArrayBuffer and TypedArray");
    }

    return new SeekableStreamReference(stream);
}

class SeekableStreamReference {

    #blob;
    #arrBuffer;
    #len;

    constructor(stream) {
        if (stream instanceof ArrayBuffer || ArrayBuffer.isView(stream)) {
            this.#arrBuffer = stream;
            this.#len = stream.byteLength;
        } else if (stream instanceof Blob) {
            this.#blob = stream;
            this.#len = stream.size;
        } else {
            throw new Error("Input is not an ArrayBuffer");
        }
    }

    async readAsync(from, count) {
        let arr;

        if (this.#blob) {
            const slice = this.#blob.slice(from, from + count);
            arr = await slice.arrayBuffer();
        } else if (this.#arrBuffer) {
            arr = this.#arrBuffer.slice(from, from + count);
        } else {
            throw new Error("Invalid state");
        }
        
        return new Uint8Array(arr);
    }

    getLength() {
        return this.#len;
    }

}