
function initFiledropJs(instance, callback) {
    const fileInput = document.getElementById('file-input');

    fileInput.addEventListener('change', async (event) => {
        event.preventDefault();
        const files = event.target.files;
        const file = files[0];
        console.log(`filename: ${file.name}`);
        console.log(`file size: ${file.size} bytes`);
        console.log(`file type: ${file.type}`);

        // Check if a file is actually present
        if (file) {
            var formData = new FormData();
            formData.append("file", file);

            try {
                fetch("https://localhost:7500", {
                    method: "POST",
                    body: formData,
                })
                .then((response) => response.json())
                .then(fetchResponse => {
                    const responseStatus = fetchResponse.status;
                    console.log("Response status from server:", responseStatus);
                    console.log(fetchResponse);
                    instance.invokeMethodAsync(callback, fetchResponse);
                }).catch(error => {
                    console.error("Error:", error);
                });
            } catch (error) {
                console.error("Error:", error);
            }
        } else {
            console.log(file);
        }
    });

    fileInput.addEventListener('click', () => {
        fileInput.value = null;
    });
}