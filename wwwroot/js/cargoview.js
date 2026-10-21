
document.addEventListener("DOMContentLoaded", () => {
    const forms = document.querySelectorAll(".move-form");
    const messageBox = document.getElementById("resultMessage");

    function showMessage(message, type) {
        messageBox.textContent = message;
        messageBox.className = type;
        messageBox.style.display = "block";
    }

    forms.forEach(form => {
        form.addEventListener("submit", async event => {
            event.preventDefault();

            const button = form.querySelector("button");
            const containerId = form.querySelector(
                'input[name="id"]'
            ).value;

            if (!confirm(
                `Are you sure you want to move container ID ${containerId}?`
            )) {
                return;
            }

            button.disabled = true;
            button.textContent = "Moving...";

            try {
                const response = await fetch(form.action, {
                    method: "POST",
                    body: new FormData(form),
                    headers: {
                        "X-Requested-With": "XMLHttpRequest"
                    }
                });

                const responseText = await response.text();

                let data;

                try {
                    data = JSON.parse(responseText);
                } catch {
                    throw new Error(
                        `The server returned a non-JSON response. HTTP ${response.status}.`
                    );
                }

                if (!response.ok || data.success !== true) {
                    throw new Error(
                        data.message ||
                        `The move failed. HTTP ${response.status}.`
                    );
                }

                showMessage(
                    data.message || "Container moved successfully.",
                    "success"
                );

                // Refresh the list after a successful move.
                setTimeout(() => window.location.reload(), 1200);

            } catch (error) {
                console.error("MoveContainer error:", error);

                showMessage(error.message, "error");

                button.disabled = false;
                button.textContent = "Move";
            }
        });
    });
});
