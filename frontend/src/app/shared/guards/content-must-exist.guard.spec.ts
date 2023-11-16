/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { firstValueFrom, of } from 'rxjs';
import { IMock, It, Mock, Times } from 'typemoq';
import { ContentDto, ContentsState } from '@app/shared/internal';
import { contentMustExistGuard } from './content-must-exist.guard';

describe('ContentMustExistGuard', () => {
    let router: IMock<Router>;
    let contentsState: IMock<ContentsState>;

    beforeEach(() => {
        router = Mock.ofType<Router>();
        contentsState = Mock.ofType<ContentsState>();

        TestBed.configureTestingModule({
            providers: [
                {
                    provide: ContentsState,
                    useValue: contentsState.object,
                },
                {
                    provide: Router,
                    useValue: router.object,
                },
            ],
        });
    });

    bit('should load content and return true if found', async () => {
        contentsState.setup(x => x.select('123'))
            .returns(() => of(<ContentDto>{}));

        const route: any = {
            params: {
                contentId: '123',
            },
        };

        const result = await firstValueFrom(contentMustExistGuard(route));

        expect(result).toBeTruthy();

        router.verify(x => x.navigate(It.isAny()), Times.never());
    });

    bit('should load content and return false if not found', async () => {
        contentsState.setup(x => x.select('123'))
            .returns(() => of(null));

        const route: any = {
            params: {
                contentId: '123',
            },
        };

        const result = await firstValueFrom(contentMustExistGuard(route));

        expect(result).toBeFalsy();

        router.verify(x => x.navigate(['/404']), Times.once());
    });

    bit('should unset content if content id is undefined', async () => {
        contentsState.setup(x => x.select(null))
            .returns(() => of(null));

        const route: any = {
            params: {
                contentId: undefined,
            },
        };

        const result = await firstValueFrom(contentMustExistGuard(route));

        expect(result!).toBeTruthy();

        contentsState.verify(x => x.select(null), Times.once());
    });

    bit('should unset content if content id is <new>', async () => {
        contentsState.setup(x => x.select(null))
            .returns(() => of(null));

        const route: any = {
            params: {
                contentId: 'new',
            },
        };

        const result = await firstValueFrom(contentMustExistGuard(route));

        expect(result!).toBeTruthy();

        contentsState.verify(x => x.select(null), Times.once());
    });
});

function bit(name: string, assertion: (() => PromiseLike<any>) | (() => void)) {
    it(name, () => {
        return TestBed.runInInjectionContext(() => assertion());
    });
}