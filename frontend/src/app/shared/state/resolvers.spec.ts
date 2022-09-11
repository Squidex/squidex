/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { firstValueFrom, of, throwError } from 'rxjs';
import { IMock, Mock, Times } from 'typemoq';
import { UIOptions } from '@app/framework';
import { ContentsService } from '../services/contents.service';
import { createContent } from '../services/contents.service.spec';
import { TestValues } from './_test-helpers';
import { ResolveContents } from './resolvers';

describe('ResolveContents', () => {
    const {
        app,
        appsState,
    } = TestValues;

    const uiOptions = new UIOptions({
        referencesDropdownItemCount: 100,
    });

    let contentsService: IMock<ContentsService>;
    let contentsResolver: ResolveContents;

    const contents = [
        createContent(1),
        createContent(2),
        createContent(3),
        createContent(4),
    ];

    beforeEach(() => {
        contentsService = Mock.ofType<ContentsService>();
        contentsResolver = new ResolveContents(uiOptions, appsState.object, contentsService.object);
    });

    it('should not resolve contents immediately', () => {
        const ids = ['id1', 'id2'];

        contentsService.setup(x => x.getAllContents(app, { ids }))
            .returns(() => of({ items: [contents[0], contents[1]] } as any));

        return expectAsync(firstValueFrom(contentsResolver.resolveMany(ids))).toBePending();
    });

    it('should resolve content from one request after delay', async () => {
        const ids = ['id1', 'id2'];

        contentsService.setup(x => x.getAllContents(app, { ids }))
            .returns(() => of({ items: [contents[0], contents[1]] } as any));

        const result = await firstValueFrom(contentsResolver.resolveMany(ids));

        expect(result.items).toEqual([
            contents[0],
            contents[1],
        ]);
    });

    it('should resolve content if not found', async () => {
        const ids = ['id1', 'id2'];

        contentsService.setup(x => x.getAllContents(app, { ids }))
            .returns(() => of({ items: [contents[0]] } as any));

        const result = await firstValueFrom(contentsResolver.resolveMany(ids));

        expect(result.items).toEqual([
            contents[0],
        ]);
    });

    it('should resolve errors', () => {
        const ids = ['id1', 'id2'];

        contentsService.setup(x => x.getAllContents(app, { ids }))
            .returns(() => throwError(() => new Error('error')));

        return expectAsync(firstValueFrom(contentsResolver.resolveMany(ids))).toBeRejected();
    });

    it('should batch results', async () => {
        const ids1 = ['id1', 'id2'];
        const ids2 = ['id2', 'id3'];

        const ids = ['id1', 'id2', 'id3'];

        contentsService.setup(x => x.getAllContents(app, { ids }))
            .returns(() => of({ items: [contents[0], contents[1], contents[2]] } as any));

        const result1Promise = firstValueFrom(contentsResolver.resolveMany(ids1));
        const result2Promise = firstValueFrom(contentsResolver.resolveMany(ids2));

        const [result1, result2] = await Promise.all([result1Promise, result2Promise]);

        expect(result1.items).toEqual([
            contents[0],
            contents[1],
        ]);

        expect(result2.items).toEqual([
            contents[1],
            contents[2],
        ]);

        contentsService.verify(x => x.getAllContents(app, { ids }), Times.once());
    });

    it('should cache results for parallel requests', async () => {
        const ids = ['id1', 'id2'];

        contentsService.setup(x => x.getAllContents(app, { ids }))
            .returns(() => of({ items: [contents[0], contents[1]] } as any));

        const result1Promise = firstValueFrom(contentsResolver.resolveMany(ids));
        const result2Promise = firstValueFrom(contentsResolver.resolveMany(ids));

        const [result1, result2] = await Promise.all([result1Promise, result2Promise]);

        expect(result1.items).toEqual([
            contents[0],
            contents[1],
        ]);

        expect(result2.items).toEqual([
            contents[0],
            contents[1],
        ]);

        contentsService.verify(x => x.getAllContents(app, { ids }), Times.once());
    });

    it('should cache results', async () => {
        const ids = ['id1', 'id2'];

        contentsService.setup(x => x.getAllContents(app, { ids }))
            .returns(() => of({ items: [contents[0], contents[1]] } as any));

        const result1 = await firstValueFrom(contentsResolver.resolveMany(ids));
        const result2 = await firstValueFrom(contentsResolver.resolveMany(ids));

        expect(result1.items).toEqual([
            contents[0],
            contents[1],
        ]);

        expect(result2.items).toEqual([
            contents[0],
            contents[1],
        ]);

        contentsService.verify(x => x.getAllContents(app, { ids }), Times.once());
    });

    it('should resolve all contents', async () => {
        const schema = 'schema1';

        contentsService.setup(x => x.getContents(app, schema, { take: 100 }))
            .returns(() => of({ items: [contents[0]] } as any));

        const result = await firstValueFrom(contentsResolver.resolveAll('schema1'));

        expect(result.items).toEqual([
            contents[0],
        ]);
    });

    it('should cache all contents for parallel requests', async () => {
        const schema = 'schema1';

        contentsService.setup(x => x.getContents(app, schema, { take: 100 }))
            .returns(() => of({ items: [contents[0]] } as any));

        const result1Promise = await firstValueFrom(contentsResolver.resolveAll('schema1'));
        const result2Promise = await firstValueFrom(contentsResolver.resolveAll('schema1'));

        const [result1, result2] = await Promise.all([result1Promise, result2Promise]);

        expect(result1.items).toEqual([
            contents[0],
        ]);

        expect(result2.items).toEqual([
            contents[0],
        ]);
    });

    it('should cache all contents', async () => {
        const schema = 'schema1';

        contentsService.setup(x => x.getContents(app, schema, { take: 100 }))
            .returns(() => of({ items: [contents[0]] } as any));

        const result1 = await firstValueFrom(contentsResolver.resolveAll('schema1'));
        const result2 = await firstValueFrom(contentsResolver.resolveAll('schema1'));

        expect(result1.items).toEqual([
            contents[0],
        ]);

        expect(result2.items).toEqual([
            contents[0],
        ]);
    });
});
