/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Router } from '@angular/router';
import { ContentDto, ContentsState } from '@app/shared/internal';
import { of } from 'rxjs';
import { IMock, It, Mock, Times } from 'typemoq';
import { ContentMustExistGuard } from './content-must-exist.guard';

describe('ContentMustExistGuard', () => {
    const route: any = {
        params: {
            contentId: '123'
        }
    };

    let router: IMock<Router>;
    let contentsState: IMock<ContentsState>;
    let contentGuard: ContentMustExistGuard;

    beforeEach(() => {
        router = Mock.ofType<Router>();
        contentsState = Mock.ofType<ContentsState>();
        contentGuard = new ContentMustExistGuard(contentsState.object, router.object);
    });

    it('should load content and return true when found', () => {
        contentsState.setup(x => x.select('123'))
            .returns(() => of(<ContentDto>{}));

        let result: boolean;

        contentGuard.canActivate(route).subscribe(x => {
            result = x;
        }).unsubscribe();

        expect(result!).toBeTruthy();

        router.verify(x => x.navigate(It.isAny()), Times.never());
    });

    it('should load content and return false when not found', () => {
        contentsState.setup(x => x.select('123'))
            .returns(() => of(null));

        let result: boolean;

        contentGuard.canActivate(route).subscribe(x => {
            result = x;
        }).unsubscribe();

        expect(result!).toBeFalsy();

        router.verify(x => x.navigate(['/404']), Times.once());
    });
});