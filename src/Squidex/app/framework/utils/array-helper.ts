/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

export module ArrayHelper {
    export function push<T>(array: T[], item: T): T[] {
        return [...array, item];
    }

    export function remove<T>(array: T[], item: T): T[] {
        const index = array.indexOf(item);

        if (index >= 0) {
            return [...array.slice(0, index), ...array.slice(index + 1)];
        } else {
            return array;
        }
    }

    export function removeAll<T>(array: T[], predicate: (item: T, index: number) => boolean): T[] {
        const copy = array.slice();

        let hasChange = false;

        for (let i = 0; i < copy.length; ) {
            if (predicate(copy[i], i)) {
                copy.splice(i, 1);

                hasChange = true;
            } else {
                ++i;
            }
        }

        return hasChange ? copy : array;
    }

    export function replace<T>(array: T[], oldItem: T, newItem: T): T[] {
        const index = array.indexOf(oldItem);

        if (index >= 0) {
            const copy = array.slice();

            copy[index] = newItem;

            return copy;
        } else {
            return array;
        }
    }

    export function replaceAll<T>(array: T[], predicate: (item: T, index: number) => boolean, replacer: (item: T) => T): T[] {
        const copy = array.slice();

        let hasChange = false;

        for (let i = 0; i < copy.length; i++) {
            if (predicate(copy[i], i)) {
                const newItem = replacer(copy[i]);

                if (copy[i] !== newItem) {
                    copy[i] = newItem;

                    hasChange = true;
                }
            }
        }

        return hasChange ? copy : array;
    }
}