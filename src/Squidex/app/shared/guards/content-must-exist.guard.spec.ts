/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Router } from '@angular/router';
import { of } from 'rxjs';
import { IMock, Mock, Times } from 'typemoq';

import { ContentDto } from './../services/contents.service';
import { ContentsState } from './../state/contents.state';
import { ContentMustExistGuard } from './content-must-exist.guard';

describe('ContentMustExistGuard', () => {
    const route: any = {
        params: {
            contentId: '123'
        }
    };

    let contentsState: IMock<ContentsState>;
    let router: IMock<Router>;
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

        contentsState.verify(x => x.select('123'), Times.once());
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