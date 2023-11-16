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
import { RuleDto, RulesState } from '@app/shared/internal';
import { ruleMustExistGuard } from './rule-must-exist.guard';

describe('RuleMustExistGuard', () => {
    let router: IMock<Router>;
    let rulesState: IMock<RulesState>;

    beforeEach(() => {
        router = Mock.ofType<Router>();
        rulesState = Mock.ofType<RulesState>();

        TestBed.configureTestingModule({
            providers: [
                {
                    provide: Router,
                    useValue: router.object,
                },
                {
                    provide: RulesState,
                    useValue: rulesState.object,
                },
            ],
        });
    });

    bit('should load rule and return true if found', async () => {
        rulesState.setup(x => x.select('123'))
            .returns(() => of(<RuleDto>{}));

        const route: any = {
            params: {
                ruleId: '123',
            },
        };

        const result = await firstValueFrom(ruleMustExistGuard(route));

        expect(result).toBeTruthy();

        router.verify(x => x.navigate(It.isAny()), Times.never());
    });

    bit('should load rule and return false if not found', async () => {
        rulesState.setup(x => x.select('123'))
            .returns(() => of(null));

        const route: any = {
            params: {
                ruleId: '123',
            },
        };

        const result = await firstValueFrom(ruleMustExistGuard(route));

        expect(result).toBeFalsy();

        router.verify(x => x.navigate(['/404']), Times.once());
    });

    bit('should unset rule if rule id is undefined', async () => {
        rulesState.setup(x => x.select(null))
            .returns(() => of(null));

        const route: any = {
            params: {
                ruleId: undefined,
            },
        };

        const result = await firstValueFrom(ruleMustExistGuard(route));

        expect(result).toBeTruthy();

        rulesState.verify(x => x.select(null), Times.once());
    });

    bit('should unset rule if rule id is <new>', async () => {
        rulesState.setup(x => x.select(null))
            .returns(() => of(null));

        const route: any = {
            params: {
                ruleId: 'new',
            },
        };

        const result = await firstValueFrom(ruleMustExistGuard(route));

        expect(result).toBeTruthy();

        rulesState.verify(x => x.select(null), Times.once());
    });
});

function bit(name: string, assertion: (() => PromiseLike<any>) | (() => void)) {
    it(name, () => {
        return TestBed.runInInjectionContext(() => assertion());
    });
}