/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { BehaviorSubject } from 'rxjs';
import { IMock, Mock, Times } from 'typemoq';
import { Queries, SavedQuery, UIState } from '@app/shared/internal';

describe('Queries', () => {
    const prefix = 'schemas.my-schema';

    let uiState: IMock<UIState>;

    let queries: Queries;

    beforeEach(() => {
        uiState = Mock.ofType<UIState>();

        const shared$ = new BehaviorSubject({
            key1: '{ "fullText": "shared1" }',
        });

        const user$ = new BehaviorSubject({
            key1: '{ "fullText": "user1" }',
        });

        const merged$ = new BehaviorSubject({
            key1: '{ "fullText": "merged1" }',
            key2: 'merged2',
            key3: undefined,
        });

        uiState.setup(x => x.get('schemas.my-schema.queries', {}))
            .returns(() => merged$);

        uiState.setup(x => x.getShared('schemas.my-schema.queries', {}))
            .returns(() => shared$);

        uiState.setup(x => x.getUser('schemas.my-schema.queries', {}))
            .returns(() => user$);

        queries = new Queries(uiState.object, prefix);
    });

    it('should load merged queries', () => {
        let converted: ReadonlyArray<SavedQuery>;

        queries.queries.subscribe(x => {
            converted = x;
        });

        expect(converted!).toEqual([
            {
                name: 'key1', query: { fullText: 'merged1' },
            }, {
                name: 'key2', query: { fullText: 'merged2' },
            }, {
                name: 'key3', query: undefined,
            },
        ]);
    });

    it('should load shared queries', () => {
        let converted: ReadonlyArray<SavedQuery>;

        queries.queriesShared.subscribe(x => {
            converted = x;
        });

        expect(converted!).toEqual([
            {
                name: 'key1', query: { fullText: 'shared1' },
            },
        ]);
    });

    it('should load user queries', () => {
        let converted: ReadonlyArray<SavedQuery>;

        queries.queriesUser.subscribe(x => {
            converted = x;
        });

        expect(converted!).toEqual([
            {
                name: 'key1', query: { fullText: 'user1' },
            },
        ]);
    });

    it('should provide key', () => {
        let key: string;

        queries.getSaveKey({}).subscribe(x => {
            key = x!;
        });

        expect(key!).toEqual('key3');
    });

    it('should forward add call to state', () => {
        queries.add('key3', { fullText: 'text3' }, true);

        expect(true).toBeTruthy();

        uiState.verify(x => x.set('schemas.my-schema.queries.key3', '{"fullText":"text3"}', true), Times.once());
    });

    it('should forward remove shared call to state', () => {
        queries.removeShared({ name: 'key3' });

        expect(true).toBeTruthy();

        uiState.verify(x => x.removeShared('schemas.my-schema.queries.key3'), Times.once());
    });

    it('should forward remove user call to state', () => {
        queries.removeUser({ name: 'key3' });

        expect(true).toBeTruthy();

        uiState.verify(x => x.removeUser('schemas.my-schema.queries.key3'), Times.once());
    });
});
