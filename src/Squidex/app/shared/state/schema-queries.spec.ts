/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { BehaviorSubject } from 'rxjs';
import { IMock,  Mock, Times } from 'typemoq';

import { Query, SchemaQueries } from './schema-queries';
import { UIState } from './ui.state';

describe('SchemaQueries', () => {
    const schema = 'my-schema';

    let uiState: IMock<UIState>;
    let filter = new BehaviorSubject('');
    let queries = new BehaviorSubject({});
    let schemaQueries: SchemaQueries;

    beforeEach(() => {
        uiState = Mock.ofType<UIState>();

        uiState.setup(x => x.get('schemas.my-schema.queries', {}))
            .returns(() => queries);

        queries.next({
            key1: 'query1',
            key2: 'query2'
        });

        schemaQueries = new SchemaQueries(uiState.object, schema);
    });

    it('should load queries', () => {
        let converted: Query[];

        schemaQueries.queries.subscribe(x => {
            converted = x;
        });

        expect(converted!).toEqual([
            {
                name: 'key1',
                nameSortable: 'KEY1',
                filter: 'query1'
            }, {
                name: 'key2',
                nameSortable: 'KEY2',
                filter: 'query2'
            }
        ]);
    });

    it('should provide key', () => {
        let key: string;

        schemaQueries.getSaveKey(filter).subscribe(x => {
            key = x!;
        });

        filter.next('query2');

        expect(key!).toEqual('key2');
    });

    it('should forward add call to state', () => {
        schemaQueries.add('key3', 'filter3');

        uiState.verify(x => x.set('schemas.my-schema.queries.key3', 'filter3'), Times.once());
    });

    it('should forward remove call to state', () => {
        schemaQueries.remove('key3');

        uiState.verify(x => x.remove('schemas.my-schema.queries.key3'), Times.once());
    });
});