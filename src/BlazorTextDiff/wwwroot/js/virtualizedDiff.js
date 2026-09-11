export function connect(left, right) {
    if (!(left instanceof HTMLElement) || !(right instanceof HTMLElement)) {
        throw new TypeError("Virtualized diff scrolling requires both pane elements.");
    }

    right.scrollTop = left.scrollTop;
    right.scrollLeft = left.scrollLeft;

    const axes = ["scrollTop", "scrollLeft"];
    const leftPosition = { scrollTop: left.scrollTop, scrollLeft: left.scrollLeft };
    const rightPosition = { scrollTop: right.scrollTop, scrollLeft: right.scrollLeft };

    const sync = (source, target, sourcePosition, targetPosition) => {
        for (const axis of axes) {
            const offset = source[axis];
            if (offset === sourcePosition[axis]) continue;

            sourcePosition[axis] = offset;
            if (target[axis] !== offset) target[axis] = offset;

            // A shorter pane clamps its offset. Ignore that reflected event, not the user's offset.
            targetPosition[axis] = target[axis];
        }
    };
    const onLeftScroll = () => sync(left, right, leftPosition, rightPosition);
    const onRightScroll = () => sync(right, left, rightPosition, leftPosition);

    left.addEventListener("scroll", onLeftScroll, { passive: true });
    right.addEventListener("scroll", onRightScroll, { passive: true });

    // Retain the elements for cleanup even after Blazor removes them from the DOM.
    return {
        dispose() {
            left.removeEventListener("scroll", onLeftScroll);
            right.removeEventListener("scroll", onRightScroll);
        }
    };
}
