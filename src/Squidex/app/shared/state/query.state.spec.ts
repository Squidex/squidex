/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import {
    LanguageDto,
    Query,
    QueryState,
    SortMode
} from '@app/shared/internal';

describe('QueryState', () => {
    let query: Query | undefined;
    let fullText: string | undefined;
    let filterState: QueryState;

    beforeEach(() => {
        filterState = new QueryState();
        filterState.setLanguage(new LanguageDto('de', 'German'));

        filterState.query.subscribe(value => {
            query = value;
        });

        filterState.fullText.subscribe(value => {
            fullText = value;
        });
    });

    it('should parse full text from query', () => {
        filterState.setQuery({ fullText: 'text' });

        expect(fullText).toBe('text');
    });

    it('should set field name and calculate query', () => {
        filterState.setFullText('text');
        filterState.setOrderField('field', 'descending');

        expect(query!).toEqual({
            sort: [
                { path: 'field', order: 'descending' }
            ],
            fullText: 'text'
        });
    });

    it('should set field and calculate query', () => {
        filterState.setFullText('text');
        filterState.setOrderField(<any>{ name: 'first-name', isLocalizable: false }, 'ascending');

        expect(query!).toEqual({
            sort: [
                { path: 'data.first-name.iv', order: 'ascending' }
            ],
            fullText: 'text'
        });
    });

    it('should set localizable field and calculate query', () => {
        filterState.setFullText('text');
        filterState.setOrderField(<any>{ name: 'first-name', isLocalizable: true }, 'ascending');

        expect(query!).toEqual({
            sort: [
                { path: 'data.first-name.de', order: 'ascending' }
            ],
            fullText: 'text'
        });
    });

    it('should update field ordering for path', () => {
        let sorting: SortMode | null;

        filterState.sortMode('field').subscribe(value => {
            sorting = value;
        });

        filterState.setQuery({ sort: [{ path: 'field', order: 'descending' }]});

        expect(sorting!).toBe('descending');
    });

    it('should update field ordering for field', () => {
        let sorting: SortMode | null;

        filterState.sortMode(<any>{ name: 'first-name', fieldId: 1, isLocalizable: false }).subscribe(value => {
            sorting = value;
        });

        filterState.setQuery({ sort: [{ path: 'data.first-name.iv', order: 'ascending' }]});

        expect(sorting!).toBe('ascending');
    });

    it('should update field ordering for localizable field', () => {
        let sorting: SortMode | null;

        filterState.sortMode(<any>{ name: 'first-name', fieldId: 1, isLocalizable: true }).subscribe(value => {
            sorting = value;
        });

        filterState.setQuery({ sort: [{ path: 'data.first-name.de', order: 'ascending' }]});

        expect(sorting!).toBe('ascending');
    });

    it('should update field ordering for localizable field and other language', () => {
        let sorting: SortMode | null;

        filterState.sortMode(<any>{ name: 'first-name', fieldId: 1, isLocalizable: true }).subscribe(value => {
            sorting = value;
        });

        filterState.setQuery({ sort: [{ path: 'data.first-name.iv', order: 'ascending' }]});

        expect(sorting!).toBeNull();
    });
});