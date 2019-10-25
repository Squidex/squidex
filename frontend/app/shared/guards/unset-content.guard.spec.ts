/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { of } from 'rxjs';
import { IMock, Mock, Times } from 'typemoq';

import { ContentsState } from '@app/shared/internal';
import { UnsetContentGuard } from './unset-content.guard';

describe('UnsetContentGuard', () => {
    let contentsState: IMock<ContentsState>;
    let contentGuard: UnsetContentGuard;

    beforeEach(() => {
        contentsState = Mock.ofType<ContentsState>();
        contentGuard = new UnsetContentGuard(contentsState.object);
    });

    it('should unset content', () => {
        contentsState.setup(x => x.select(null))
            .returns(() => of(null));

        let result: boolean;

        contentGuard.canActivate().subscribe(x => {
            result = x;
        }).unsubscribe();

        expect(result!).toBeTruthy();

        contentsState.verify(x => x.select(null), Times.once());
    });
});