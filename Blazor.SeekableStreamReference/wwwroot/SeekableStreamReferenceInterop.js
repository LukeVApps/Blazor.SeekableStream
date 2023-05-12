export async function createStreamReference(stream) {
    if (stream instanceof Blob) {
        stream = await stream.arrayBuffer();
    } else if (stream instanceof Response) {
        stream = await stream.arrayBuffer();
    } else {
        throw new Error("Invalid Stream input. This method accepts Blob, Response, ArrayBuffer and TypedArray");
    }

    return new SeekableStreamReference(stream);
}

class SeekableStreamReference {

    #arrBuffer;
    #len;

    constructor(stream) {
        if (stream instanceof ArrayBuffer || ArrayBuffer.isView(stream)) {
            this.#arrBuffer = stream;
            this.#len = stream.byteLength;
        } else {
            throw new Error("Input is not an ArrayBuffer");
        }
    }

    readAsync(from, count) {
        var result = this.#arrBuffer.slice(from, from + count);
        return new Uint8Array(result);
    }

    getLength() {
        return this.#len;
    }

}