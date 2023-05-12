export function pickFileAsync() {
    return new Promise(r => {
        const txt = document.createElement("input");
        txt.type = "file";
        txt.onchange = () => r(txt.files[0]);
        txt.click();
    });
}