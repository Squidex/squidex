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
        const updated = state.next({ value: 1 });

        expect(updated).toBeTruthy();
        expect(state.value()).toEqual({ value: 1 });
        expect(state.lastChange().event).toEqual('Update');
    });

    it('should reset state with new value', () => {
        const updated = state.resetState({ value: 1 });

        expect(updated).toBeTruthy();
        expect(state.value()).toEqual({ value: 1 });
        expect(state.lastChange().event).toEqual('Reset');
    });

    it('should not update state if nothing changed', () => {
        state.next({ value: 1 });

        const change = state.lastChange();
        const updated = state.next({ value: 1 });

        expect(updated).toBeFalsy();
        expect(state.lastChange()).toBe(change);
    });

    it('should not reset state if nothing changed', () => {
        state.resetState({ value: 1 });

        const change = state.lastChange();
        const updated = state.resetState({ value: 1 });

        expect(updated).toBeFalsy();
        expect(state.lastChange()).toBe(change);
    });

    it('should provide snapshot', () => {
        state.next({ value: 1 });

        expect(state.snapshot).toEqual({ value: 1 });
    });
});
