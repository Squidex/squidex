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
    let router: IMock<Router>;
    let rulesState: IMock<RulesState>;
    let ruleGuard: RuleMustExistGuard;

    beforeEach(() => {
        router = Mock.ofType<Router>();
        rulesState = Mock.ofType<RulesState>();
        ruleGuard = new RuleMustExistGuard(rulesState.object, router.object);
    });

    it('should load rule and return true if found', () => {
        rulesState.setup(x => x.select('123'))
            .returns(() => of(<RuleDto>{}));

        let result: boolean;

        const route: any = {
            params: {
                ruleId: '123',
            },
        };

        ruleGuard.canActivate(route).subscribe(x => {
            result = x;
        }).unsubscribe();

        expect(result!).toBeTruthy();

        router.verify(x => x.navigate(It.isAny()), Times.never());
    });

    it('should load rule and return false if not found', () => {
        rulesState.setup(x => x.select('123'))
            .returns(() => of(null));

        let result: boolean;

        const route: any = {
            params: {
                ruleId: '123',
            },
        };

        ruleGuard.canActivate(route).subscribe(x => {
            result = x;
        }).unsubscribe();

        expect(result!).toBeFalsy();

        router.verify(x => x.navigate(['/404']), Times.once());
    });

    it('should unset rule if rule id is undefined', () => {
        rulesState.setup(x => x.select(null))
            .returns(() => of(null));

        let result: boolean;

        const route: any = {
            params: {
                ruleId: undefined,
            },
        };

        ruleGuard.canActivate(route).subscribe(x => {
            result = x;
        }).unsubscribe();

        expect(result!).toBeTruthy();

        rulesState.verify(x => x.select(null), Times.once());
    });

    it('should unset rule if rule id is <new>', () => {
        rulesState.setup(x => x.select(null))
            .returns(() => of(null));

        let result: boolean;

        const route: any = {
            params: {
                ruleId: 'new',
            },
        };

        ruleGuard.canActivate(route).subscribe(x => {
            result = x;
        }).unsubscribe();

        expect(result!).toBeTruthy();

        rulesState.verify(x => x.select(null), Times.once());
    });
});
