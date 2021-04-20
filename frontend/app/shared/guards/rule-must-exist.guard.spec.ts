/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Router } from '@angular/router';
import { RuleDto, RulesState } from '@app/shared/internal';
import { of } from 'rxjs';
import { IMock, It, Mock, Times } from 'typemoq';
import { RuleMustExistGuard } from './rule-must-exist.guard';

describe('RuleMustExistGuard', () => {
    const route: any = {
        params: {
            ruleId: '123'
        }
    };

    let router: IMock<Router>;
    let rulesState: IMock<RulesState>;
    let ruleGuard: RuleMustExistGuard;

    beforeEach(() => {
        router = Mock.ofType<Router>();
        rulesState = Mock.ofType<RulesState>();
        ruleGuard = new RuleMustExistGuard(rulesState.object, router.object);
    });

    it('should load rule and return true when found', () => {
        rulesState.setup(x => x.select('123'))
            .returns(() => of(<RuleDto>{}));

        let result: boolean;

        ruleGuard.canActivate(route).subscribe(x => {
            result = x;
        }).unsubscribe();

        expect(result!).toBeTruthy();

        router.verify(x => x.navigate(It.isAny()), Times.never());
    });

    it('should load rule and return false when not found', () => {
        rulesState.setup(x => x.select('123'))
            .returns(() => of(null));

        let result: boolean;

        ruleGuard.canActivate(route).subscribe(x => {
            result = x;
        }).unsubscribe();

        expect(result!).toBeFalsy();

        router.verify(x => x.navigate(['/404']), Times.once());
    });
});