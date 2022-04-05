/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { SchemaScriptNamePipe } from './schema-scripts.pipes';

describe('SchemaScriptsPipe', () => {
    const pipe = new SchemaScriptNamePipe();

    it('should return titlecase for schema name', () => {
        const actual = pipe.transform('create');

        expect(actual).toEqual('Create');
    });

    it('should return custom name for queryPre', () => {
        const actual = pipe.transform('queryPre');

        expect(actual).toEqual('Prepare Query');
    });
});