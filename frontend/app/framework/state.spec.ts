/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { State } from './state';

describe('State', () => {
    let state: State<any>;

    beforeEach(() => {
        state = new State<any>({});
    });

    it('should update state with new value', () => {
        let updateCount = 0;

        state.changes.subscribe(() => {
            updateCount++;
        });

        const updated = state.next({ value: 1 });

        expect(updateCount).toEqual(2);
        expect(updated).toBeTruthy();
    });

    it('should reset state with new value', () => {
        let updateCount = 0;

        state.changes.subscribe(() => {
            updateCount++;
        });

        const updated = state.resetState({ value: 1 });

        expect(updateCount).toEqual(2);
        expect(updated).toBeTruthy();
    });

    it('should not update state if nothing changed', () => {
        let updateCount = 0;

        state.changes.subscribe(() => {
            updateCount++;
        });

        state.next({ value: 1 });

        const updated = state.next({ value: 1 });

        expect(updateCount).toEqual(2);
        expect(updated).toBeFalsy();
    });

    it('should not reset state if nothing changed', () => {
        let updateCount = 0;

        state.changes.subscribe(() => {
            updateCount++;
        });

        state.resetState({ value: 1 });

        const updated = state.resetState({ value: 1 });

        expect(updateCount).toEqual(2);
        expect(updated).toBeFalsy();
    });
});
