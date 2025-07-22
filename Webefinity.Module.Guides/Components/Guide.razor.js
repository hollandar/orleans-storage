// MyComponent.razor.js

export function adjustAnchors(guideElement, dotNetHelper) {
    if (guideElement) {
        const anchors = guideElement.querySelectorAll('a[href^="@"]:not(.redirected)');
        anchors.forEach(anchor => {
            anchor.addEventListener('click', async (event) => {
                event.preventDefault();
                const targetId = anchor.getAttribute('href').substring(1); // Remove the leading '@'
                await dotNetHelper.invokeMethodAsync('VisitAnchorAsync', targetId);
            });
            anchor.classList.add('redirected');
        });
    }
}

export function windowResize() {
    if (window && window.dispatchEvent) {
        const event = new Event('resize');
        window.dispatchEvent(event);
    }
}

