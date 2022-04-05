/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Router } from '@angular/router';
import { firstValueFrom, of } from 'rxjs';
import { IMock, It, Mock, Times } from 'typemoq';
import { RuleDto, RulesState } from '@app/shared/internal';
import { RuleMustExistGuard } from './rule-must-exist.guard';

describe('RuleMustExistGuard', () => {
    let router: IMock<Router>;
    let rulesState: IMock<RulesState>;
    let ruleGuard: RuleMustExistGuard;

    beforeEach(() => {
        router = Mock.ofType<Router>();
        rulesState = Mock.ofType<RulesState>();
        ruleGuard = new RuleMustExistGuard(rulesState.object, router.object);
    });

    it('should load rule and return true if found', async () => {
        rulesState.setup(x => x.select('123'))
            .returns(() => of(<RuleDto>{}));

        const route: any = {
            params: {
                ruleId: '123',
            },
        };

        const result = await firstValueFrom(ruleGuard.canActivate(route));

        expect(result).toBeTruthy();

        router.verify(x => x.navigate(It.isAny()), Times.never());
    });

    it('should load rule and return false if not found', async () => {
        rulesState.setup(x => x.select('123'))
            .returns(() => of(null));

        const route: any = {
            params: {
                ruleId: '123',
            },
        };

        const result = await firstValueFrom(ruleGuard.canActivate(route));

        expect(result).toBeFalsy();

        router.verify(x => x.navigate(['/404']), Times.once());
    });

    it('should unset rule if rule id is undefined', async () => {
        rulesState.setup(x => x.select(null))
            .returns(() => of(null));

        const route: any = {
            params: {
                ruleId: undefined,
            },
        };

        const result = await firstValueFrom(ruleGuard.canActivate(route));

        expect(result).toBeTruthy();

        rulesState.verify(x => x.select(null), Times.once());
    });

    it('should unset rule if rule id is <new>', async () => {
        rulesState.setup(x => x.select(null))
            .returns(() => of(null));

        const route: any = {
            params: {
                ruleId: 'new',
            },
        };

        const result = await firstValueFrom(ruleGuard.canActivate(route));

        expect(result).toBeTruthy();

        rulesState.verify(x => x.select(null), Times.once());
    });
});
