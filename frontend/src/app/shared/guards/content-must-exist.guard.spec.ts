/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Router } from '@angular/router';
import { firstValueFrom, of } from 'rxjs';
import { IMock, It, Mock, Times } from 'typemoq';
import { ContentDto, ContentsState } from '@app/shared/internal';
import { ContentMustExistGuard } from './content-must-exist.guard';

describe('ContentMustExistGuard', () => {
    let router: IMock<Router>;
    let contentsState: IMock<ContentsState>;
    let contentGuard: ContentMustExistGuard;

    beforeEach(() => {
        router = Mock.ofType<Router>();
        contentsState = Mock.ofType<ContentsState>();
        contentGuard = new ContentMustExistGuard(contentsState.object, router.object);
    });

    it('should load content and return true if found', async () => {
        contentsState.setup(x => x.select('123'))
            .returns(() => of(<ContentDto>{}));

        const route: any = {
            params: {
                contentId: '123',
            },
        };

        const result = await firstValueFrom(contentGuard.canActivate(route));

        expect(result).toBeTruthy();

        router.verify(x => x.navigate(It.isAny()), Times.never());
    });

    it('should load content and return false if not found', async () => {
        contentsState.setup(x => x.select('123'))
            .returns(() => of(null));

        const route: any = {
            params: {
                contentId: '123',
            },
        };

        const result = await firstValueFrom(contentGuard.canActivate(route));

        expect(result).toBeFalsy();

        router.verify(x => x.navigate(['/404']), Times.once());
    });

    it('should unset content if content id is undefined', async () => {
        contentsState.setup(x => x.select(null))
            .returns(() => of(null));

        const route: any = {
            params: {
                contentId: undefined,
            },
        };

        const result = await firstValueFrom(contentGuard.canActivate(route));

        expect(result!).toBeTruthy();

        contentsState.verify(x => x.select(null), Times.once());
    });

    it('should unset content if content id is <new>', async () => {
        contentsState.setup(x => x.select(null))
            .returns(() => of(null));

        const route: any = {
            params: {
                contentId: 'new',
            },
        };

        const result = await firstValueFrom(contentGuard.canActivate(route));

        expect(result!).toBeTruthy();

        contentsState.verify(x => x.select(null), Times.once());
    });
});
