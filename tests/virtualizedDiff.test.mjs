import assert from "node:assert/strict";
import { after, beforeEach, test } from "node:test";
import { connect } from "../src/BlazorTextDiff/wwwroot/js/virtualizedDiff.js";

const pendingScrolls = new Set();
const originalHTMLElement = globalThis.HTMLElement;

class ScrollPane extends EventTarget {
    #top = 0;
    #left = 0;

    constructor(maxLeft = 1000, maxTop = 1000) {
        super();
        this.maxLeft = maxLeft;
        this.maxTop = maxTop;
    }

    get scrollTop() { return this.#top; }
    get scrollLeft() { return this.#left; }

    set scrollTop(value) {
        const next = Math.max(0, Math.min(value, this.maxTop));
        if (next !== this.#top) {
            this.#top = next;
            pendingScrolls.add(this);
        }
    }

    set scrollLeft(value) {
        const next = Math.max(0, Math.min(value, this.maxLeft));
        if (next !== this.#left) {
            this.#left = next;
            pendingScrolls.add(this);
        }
    }
}

globalThis.HTMLElement = ScrollPane;
beforeEach(() => pendingScrolls.clear());
after(() => {
    if (originalHTMLElement === undefined) delete globalThis.HTMLElement;
    else globalThis.HTMLElement = originalHTMLElement;
});

// Browsers coalesce scroll events and dispatch them after applying clamped offsets.
function flushScrolls() {
    for (let round = 0; pendingScrolls.size > 0; round++) {
        assert.ok(round < 20, "Scroll synchronization should settle without a feedback loop");
        const panes = [...pendingScrolls];
        pendingScrolls.clear();
        for (const pane of panes) pane.dispatchEvent(new Event("scroll"));
    }
}

test("scrolls both axes from either pane", () => {
    const left = new ScrollPane();
    const right = new ScrollPane();
    const connection = connect(left, right);

    left.scrollTop = 200;
    left.scrollLeft = 350;
    flushScrolls();
    assert.equal(right.scrollTop, 200);
    assert.equal(right.scrollLeft, 350);

    right.scrollTop = 500;
    right.scrollLeft = 125;
    flushScrolls();
    assert.equal(left.scrollTop, 500);
    assert.equal(left.scrollLeft, 125);
    connection.dispose();
});

for (const widerSide of ["left", "right"]) {
    test(`the wider ${widerSide} pane can scroll past the other pane's limit`, () => {
        const left = new ScrollPane(widerSide === "left" ? 1000 : 100);
        const right = new ScrollPane(widerSide === "right" ? 1000 : 100);
        const wider = widerSide === "left" ? left : right;
        const narrower = widerSide === "left" ? right : left;
        const connection = connect(left, right);

        wider.scrollLeft = 750;
        flushScrolls();
        assert.equal(narrower.scrollLeft, 100);
        assert.equal(wider.scrollLeft, 750);

        narrower.scrollTop = 450;
        flushScrolls();
        assert.equal(wider.scrollTop, 450);
        assert.equal(wider.scrollLeft, 750, "Vertical scrolling must not reset the other axis");

        narrower.scrollLeft = 50;
        flushScrolls();
        assert.equal(wider.scrollLeft, 50);
        connection.dispose();
    });
}

test("preserves pending changes on different axes in different panes", () => {
    const left = new ScrollPane();
    const right = new ScrollPane();
    const connection = connect(left, right);

    left.scrollLeft = 200;
    right.scrollTop = 400;
    flushScrolls();

    assert.equal(left.scrollTop, 400);
    assert.equal(right.scrollTop, 400);
    assert.equal(left.scrollLeft, 200);
    assert.equal(right.scrollLeft, 200);
    connection.dispose();
});

test("initial alignment respects the shorter pane without snapping back", () => {
    const left = new ScrollPane();
    const right = new ScrollPane(150);
    left.scrollTop = 100;
    left.scrollLeft = 600;
    const connection = connect(left, right);
    flushScrolls();

    assert.equal(right.scrollTop, 100);
    assert.equal(right.scrollLeft, 150);
    assert.equal(left.scrollLeft, 600);
    connection.dispose();
});

test("disposal stops synchronizing either axis", () => {
    const left = new ScrollPane();
    const right = new ScrollPane();
    const connection = connect(left, right);
    connection.dispose();

    left.scrollTop = 200;
    left.scrollLeft = 150;
    flushScrolls();
    assert.equal(right.scrollTop, 0);
    assert.equal(right.scrollLeft, 0);

    right.scrollTop = 500;
    right.scrollLeft = 650;
    flushScrolls();
    assert.equal(left.scrollTop, 200);
    assert.equal(left.scrollLeft, 150);
});
