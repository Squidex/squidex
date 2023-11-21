/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { TestBed } from '@angular/core/testing';
import { Mock } from 'typemoq';
import { applyAwarenessUpdate, Awareness, encodeAwarenessUpdate } from 'y-protocols/awareness';
import * as Y from 'yjs';
import { AuthService, CollaborationProvider, CollaborationService, UIOptions } from '@app/shared/internal';

describe('CollaborationService', () => {
    let collaborationService: CollaborationService;
    let collaborationProvider!: CollaborationProvider;

    beforeEach(() => {
        TestBed.configureTestingModule({
            providers: [
                {
                    provide: UIOptions,
                    useValue: new UIOptions({}),
                },
            ],
        });

        TestBed.runInInjectionContext(() => {
            collaborationService = new CollaborationService(Mock.ofType<AuthService>().object);
        });

        collaborationService.providerFactory = (_, doc) => {
            collaborationProvider = { awareness: new Awareness(doc), doc, destroy: () => {} };

            return collaborationProvider;
        };

        collaborationService.connect('my-room');
    });

    it('should also get map if disconnected', () => {
        collaborationService.connect(null);

        let map: any = undefined;
        collaborationService.getMap('map').subscribe(v => map = v);

        expect(map).toBeDefined();
    });

    it('should also get array if disconnected', () => {
        collaborationService.connect(null);

        let array: any = undefined;
        collaborationService.getArray('array').subscribe(v => array = v);

        expect(array).toBeDefined();
    });

    it('should add to map', () => {
        const map = collaborationService.getMap('map');

        let values: Record<string, any> = {};

        map.subscribe(state => {
            state.valueChanges.subscribe(v => values = v);

            state.set('key1', 41);
            state.set('key1', 42);
        });

        expect(values).toEqual({ key1: 42 });
    });

    it('should remove from map', () => {
        const map = collaborationService.getMap('map');

        let values: Record<string, any> = {};

        map.subscribe(state => {
            state.valueChanges.subscribe(v => values = v);

            state.set('key0', 41);
            state.set('key1', 42);
            state.remove('key0');
        });

        expect(values).toEqual({ key1: 42 });
    });

    it('should add to array', () => {
        const array = collaborationService.getArray('array');

        let items: ReadonlyArray<any> = [];

        array.subscribe(state => {
            state.itemsChanges.subscribe(i => items = i);

            state.add(1);
            state.add(2);
        });

        expect(items).toEqual([ 1, 2]);
    });

    it('should replace in array', () => {
        const array = collaborationService.getArray('array');

        let items: ReadonlyArray<any> = [];

        array.subscribe(state => {
            state.itemsChanges.subscribe(i => items = i);

            state.add(1);
            state.add(2);
            state.set(0, 42);
        });

        expect(items).toEqual([ 42, 2]);
    });

    it('should remove from array', () => {
        const array = collaborationService.getArray('array');

        let items: ReadonlyArray<any> = [];

        array.subscribe(state => {
            state.itemsChanges.subscribe(i => items = i);

            state.add(1);
            state.add(2);
            state.remove(0);
        });

        expect(items).toEqual([ 2]);
    });

    it('should provide one awareness per user', () => {
        let users: any[] = [];
        collaborationService.userChanges.subscribe(u => users = u);

        collaborationProvider.awareness.setLocalStateField('user', { id: '0', displayName: 'User0' });
        collaborationProvider.awareness.setLocalStateField('key1', 101);
        collaborationProvider.awareness.setLocalStateField('key2', 102);

        setOtherAwareness({
            user: { id: '1', displayName: 'User1' },
            key1: 201,
            key2: 202,
        });

        setOtherAwareness({
            user: { id: '1', displayName: 'User1' },
            key1: 301,
            key2: 302,
        });

        setOtherAwareness({
            user: { id: '2', displayName: 'User2' },
            key1: 401,
            key2: 402,
        });

        expect(users).toEqual([
            {
                user: { id: '1', displayName: 'User1' },
                key1: 301,
                key2: 302,
            },
            {
                user: { id: '2', displayName: 'User2' },
                key1: 401,
                key2: 402,
            },
        ]);
    });

    function setOtherAwareness(state: any) {
        const otherDoc = new Y.Doc();
        const otherAwarness = new Awareness(otherDoc);

        otherAwarness.setLocalState(state);

        applyAwarenessUpdate(collaborationProvider.awareness, encodeAwarenessUpdate(otherAwarness, [otherDoc.clientID], otherAwarness.getStates()), otherAwarness);
    }
});