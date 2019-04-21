/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import {
    FilterState,
    LanguageDto,
    Sorting
} from './../';

describe('FilterState', () => {
    let filterState: FilterState;
    let query: string | undefined;
    let filter: string | undefined;
    let fullText: string | undefined;
    let order: string | undefined;

    beforeEach(() => {
        filterState = new FilterState();
        filterState.setLanguage(new LanguageDto('de', 'German'));

        filterState.query.subscribe(value => {
            query = value;
        });

        filterState.filter.subscribe(value => {
            filter = value;
        });

        filterState.fullText.subscribe(value => {
            fullText = value;
        });

        filterState.order.subscribe(value => {
            order = value;
        });
    });

    it('should parse elements from query', () => {
        const newQuery = '$filter=MY_FILTER&$orderby=MY_FIELD desc&$search=MY_TEXT';

        filterState.setQuery(newQuery);

        expect(order).toBe('MY_FIELD desc');
        expect(query).toBe(newQuery);
        expect(filter).toBe('MY_FILTER');
        expect(fullText).toBe('MY_TEXT');
    });

    it('should set full text and order and calculate query', () => {
        filterState.setFullText('Hello World');
        filterState.setOrder('data/name/iv asc');

        expect(query).toBe('$search=Hello World&$orderby=data/name/iv asc');
        expect(fullText).toBe('Hello World');
    });

    it('should set full text only and calculate query', () => {
        filterState.setFullText('Hello World');

        expect(query).toBe('Hello World');
        expect(fullText).toBe('Hello World');
    });

    it('should set filter and calculate query', () => {
        filterState.setFilter('data/name/iv eq "Squidex"');

        expect(query).toBe('$filter=data/name/iv eq "Squidex"');
        expect(filter).toBe('data/name/iv eq "Squidex"');
    });

    it('should set order and calculate query', () => {
        filterState.setOrder('data/name/iv asc');

        expect(query).toBe('$orderby=data/name/iv asc');
        expect(order).toBe('data/name/iv asc');
    });

    it('should set field name and calculate query', () => {
        filterState.setOrderField('field', 'Descending');

        expect(query).toBe('$orderby=field desc');
        expect(order).toBe('field desc');
    });

    it('should set field and calculate query', () => {
        filterState.setOrderField(<any>{ name: 'first-name', isLocalizable: false }, 'Ascending');

        expect(query).toBe('$orderby=data/first_name/iv asc');
        expect(order).toBe('data/first_name/iv asc');
    });

    it('should set localizable field and calculate query', () => {
        filterState.setOrderField(<any>{ name: 'first-name', isLocalizable: true }, 'Ascending');

        expect(query).toBe('$orderby=data/first_name/de asc');
        expect(order).toBe('data/first_name/de asc');
    });

    it('should update field ordering for path', () => {
        let sorting: Sorting;

        filterState.sortMode('field').subscribe(value => {
            sorting = value;
        });

        filterState.setQuery('$orderby=field desc');

        expect(sorting!).toBe('Descending');
    });

    it('should update field ordering for field', () => {
        let sorting: Sorting;

        filterState.sortMode(<any>{ name: 'first-name', fieldId: 1, isLocalizable: false }).subscribe(value => {
            sorting = value;
        });

        filterState.setQuery('$orderby=data/first_name/iv asc');

        expect(sorting!).toBe('Ascending');
    });

    it('should update field ordering for localizable field', () => {
        let sorting: Sorting;

        filterState.sortMode(<any>{ name: 'first-name', fieldId: 1, isLocalizable: true }).subscribe(value => {
            sorting = value;
        });

        filterState.setQuery('$orderby=data/first_name/de asc');

        expect(sorting!).toBe('Ascending');
    });

    it('should update field ordering for localizable field and other language', () => {
        let sorting: Sorting;

        filterState.sortMode(<any>{ name: 'first-name', fieldId: 1, isLocalizable: true }).subscribe(value => {
            sorting = value;
        });

        filterState.setQuery('$orderby=data/first_name/en asc');

        expect(sorting!).toBe('None');
    });
});