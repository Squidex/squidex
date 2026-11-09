/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { of } from 'rxjs';
import { IMock, It, Mock } from 'typemoq';
import { createService } from '@app/framework/testing';
import { AppsState, BulkResultDto, ComponentContentsState, ContentsDto, ContentsService, DialogService } from '@app/shared/internal';
import { createContent } from '../services/contents.service.spec';
import { TestValues } from './_test-helpers';

describe('ContentsState', () => {
    const { app, appsState } = TestValues;

    const content1 = createContent(1);
    const content2 = createContent(2);
    const content3 = createContent(3);

    let dialogs: IMock<DialogService>;
    let contentsService: IMock<ContentsService>;
    let contentsState: ComponentContentsState;

    beforeEach(() => {
        dialogs = Mock.ofType<DialogService>();

        contentsService = Mock.ofType<ContentsService>();

        contentsState = createService(ComponentContentsState, [
            { provide: AppsState, useValue: appsState.object },
            { provide: ContentsService, useValue: contentsService.object },
            { provide: DialogService, useValue: dialogs.object },
        ]);
        contentsState.schema = { name: 'my-schema' };

        contentsService.setup(x => x.getContents(app, 'my-schema', It.isAny()))
            .returns(() => of(new ContentsDto({ items: [content1, content2], total: 2, statuses: [], _links: {} })));

        contentsState.load().subscribe();
    });

    it('should add created content to the beginning of the list', () => {
        contentsService.setup(x => x.postContent(app, 'my-schema', It.isAny(), false, ''))
            .returns(() => of(content3));

        contentsState.create({}, false).subscribe();

        expect(contentsState.snapshot.contents).toEqual([content3, content1, content2]);
        expect(contentsState.snapshot.total).toEqual(3);
    });

    it('should update selected content when it is not the last reloaded content', () => {
        const content1New = createContent(1, '_new');
        const content2New = createContent(2, '_new');

        contentsService.setup(x => x.getContent(app, 'my-schema', content1.id))
            .returns(() => of(content1));

        contentsService.setup(x => x.bulkUpdate(app, 'my-schema', It.isAny()))
            .returns(() => of([
                new BulkResultDto({ jobIndex: 0, contentId: content1.id }),
                new BulkResultDto({ jobIndex: 1, contentId: content2.id }),
            ]));

        contentsService.setup(x => x.getAllContents(app, It.isAny()))
            .returns(() => of(new ContentsDto({ items: [content1New, content2New], total: 2, statuses: [], _links: {} })));

        contentsState.select(content1.id).subscribe();
        contentsState.changeManyStatus([content1, content2], 'Published').subscribe();

        expect(contentsState.snapshot.selectedContent).toBe(content1New);
        expect(contentsState.snapshot.contents).toEqual([content1New, content2New]);
    });
});
