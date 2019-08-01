/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { BehaviorSubject } from 'rxjs';
import { IMock,  Mock, Times } from 'typemoq';

import {
    Queries,
    SavedQuery,
    UIState
} from '@app/shared/internal';
import { encodeQuery } from './query';

describe('Queries', () => {
    const prefix = 'schemas.my-schema';

    let uiState: IMock<UIState>;

    let filter = new BehaviorSubject('');

    let queries$ = new BehaviorSubject({});
    let queries: Queries;

    beforeEach(() => {
        uiState = Mock.ofType<UIState>();

        uiState.setup(x => x.get('schemas.my-schema.queries', {}))
            .returns(() => queries$);

        queries$.next({
            key1: '{ "fullText": "text1" }',
            key2: 'text2',
            key3: undefined
        });

        queries = new Queries(uiState.object, prefix);
    });

    it('should load queries', () => {
        let converted: SavedQuery[];

        queries.queries.subscribe(x => {
            converted = x;
        });

        expect(converted!).toEqual([
            {
                name: 'key1',
                query: { fullText: 'text1' },
                queryJson: encodeQuery({ fullText: 'text1' })
            }, {
                name: 'key2',
                query: { fullText: 'text2' },
                queryJson: encodeQuery({ fullText: 'text2' })
            }, {
                name: 'key3',
                query: undefined,
                queryJson: ''
            }
        ]);
    });

    it('should provide key', () => {
        let key: string;

        queries.getSaveKey(filter).subscribe(x => {
            key = x!;
        });

        filter.next('');

        expect(key!).toEqual('key3');
    });

    it('should forward add call to state', () => {
        queries.add('key3', { fullText: 'text3' });

        expect(true).toBeTruthy();

        uiState.verify(x => x.set('schemas.my-schema.queries.key3', '{"fullText":"text3"}'), Times.once());
    });

    it('should forward remove call to state', () => {
        queries.remove({ name: 'key3' });

        expect(true).toBeTruthy();

        uiState.verify(x => x.remove('schemas.my-schema.queries.key3'), Times.once());
    });
});