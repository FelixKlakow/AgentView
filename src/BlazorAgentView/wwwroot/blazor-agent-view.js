export function scrollToBottom(element) {
    if (element) element.scrollTop = element.scrollHeight;
}

// Returns true when the user is within ~32px of the bottom of the scroll
// container, which is our heuristic for "the user is following the latest
// content" so it's safe to auto-scroll on new messages.
export function isNearBottom(element, threshold) {
    if (!element) return true;
    const t = typeof threshold === "number" ? threshold : 32;
    const distance = element.scrollHeight - element.scrollTop - element.clientHeight;
    return distance <= t;
}
