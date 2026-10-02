// LP-007: drag & drop with SortableJS for Blazor.
// SortableJS moves DOM nodes, but Blazor owns the DOM. So every move is undone right away and reported
// to .NET; the component updates its model and Blazor renders the new state (cards need @key).
import Sortable from '../lib/sortablejs/sortable.esm.js';

export function attach(element, dotNetRef, options) {
    const sortable = Sortable.create(element, {
        group: options.group,
        sort: options.sort,
        animation: 150,
        // Finger must rest briefly before a drag starts, so the page can still be scrolled.
        delay: options.touchDelayMs,
        delayOnTouchOnly: true,
        touchStartThreshold: 5,
        ghostClass: 'dd-ghost',
        chosenClass: 'dd-chosen',
        dragClass: 'dd-drag',
        onEnd: evt => {
            const { item, from, to, oldIndex, newIndex } = evt;

            item.remove();
            from.insertBefore(item, from.children[oldIndex] ?? null);

            if (from === to && oldIndex === newIndex) {
                return;
            }

            dotNetRef.invokeMethodAsync('OnDrop', item.dataset.id, from.dataset.zone, to.dataset.zone, newIndex);
        },
    });

    return {
        dispose: () => sortable.destroy(),
    };
}
