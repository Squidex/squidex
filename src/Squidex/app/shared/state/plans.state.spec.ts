/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { of } from 'rxjs';
import { onErrorResumeNext } from 'rxjs/operators';
import { IMock, It, Mock, Times } from 'typemoq';

import {
    DialogService,
    PlanDto,
    PlansDto,
    PlansService,
    PlansState,
    Versioned
} from './../';

import { TestValues } from './_test-helpers';

describe('PlansState', () => {
    const {
        app,
        appsState,
        authService,
        newVersion,
        version
    } = TestValues;

    const oldPlans =
        new PlansDto('id1', 'id2', true, [
            new PlanDto('id1', 'name1', '100€', 'id1_yearly', '200€', 1, 1, 1),
            new PlanDto('id2', 'name2', '400€', 'id2_yearly', '800€', 2, 2, 2)
        ],
        version);

    let dialogs: IMock<DialogService>;
    let plansService: IMock<PlansService>;
    let plansState: PlansState;

    beforeEach(() => {
        dialogs = Mock.ofType<DialogService>();

        plansService = Mock.ofType<PlansService>();

        plansService.setup(x => x.getPlans(app))
            .returns(() => of(oldPlans));

        plansState = new PlansState(appsState.object, authService.object, dialogs.object, plansService.object);
    });

    it('should load plans', () => {
        plansState.load().pipe(onErrorResumeNext()).subscribe();

        expect(plansState.snapshot.plans.values).toEqual([
            { isSelected: true,  isYearlySelected: false, plan: oldPlans.plans[0] },
            { isSelected: false, isYearlySelected: false, plan: oldPlans.plans[1] }
        ]);
        expect(plansState.snapshot.isOwner).toBeFalsy();
        expect(plansState.snapshot.isLoaded).toBeTruthy();
        expect(plansState.snapshot.hasPortal).toBeTruthy();
        expect(plansState.snapshot.version).toEqual(version);

        dialogs.verify(x => x.notifyInfo(It.isAnyString()), Times.never());
    });

    it('should load plans with overriden id', () => {
        plansState.load(false, 'id2_yearly').pipe(onErrorResumeNext()).subscribe();

        expect(plansState.snapshot.plans.values).toEqual([
            { isSelected: false, isYearlySelected: false, plan: oldPlans.plans[0] },
            { isSelected: false, isYearlySelected: true,  plan: oldPlans.plans[1] }
        ]);
        expect(plansState.snapshot.isOwner).toBeFalsy();
        expect(plansState.snapshot.isLoaded).toBeTruthy();
        expect(plansState.snapshot.hasPortal).toBeTruthy();
        expect(plansState.snapshot.version).toEqual(version);

        dialogs.verify(x => x.notifyInfo(It.isAnyString()), Times.never());
    });

    it('should show notification on load when reload is true', () => {
        plansState.load(true).subscribe();

        expect().nothing();

        dialogs.verify(x => x.notifyInfo(It.isAnyString()), Times.once());
    });

    it('should redirect when returning url', () => {
        plansState.window = <any>{ location: {} };

        const result = { redirectUri: 'http://url' };

        plansService.setup(x => x.putPlan(app, It.isAny(), version))
            .returns(() => of(new Versioned(newVersion, result)));

        plansState.load().subscribe();
        plansState.change('free').pipe(onErrorResumeNext()).subscribe();

        expect(plansState.snapshot.plans.values).toEqual([
            { isSelected: true,  isYearlySelected: false, plan: oldPlans.plans[0] },
            { isSelected: false, isYearlySelected: false, plan: oldPlans.plans[1] }
        ]);
        expect(plansState.window.location.href).toBe(result.redirectUri);
        expect(plansState.snapshot.version).toEqual(version);
    });

    it('should update plans when no returning url', () => {
        plansState.window = <any>{ location: {} };

        plansService.setup(x => x.putPlan(app, It.isAny(), version))
            .returns(() => of(new Versioned(newVersion, { redirectUri: '' })));

        plansState.load().subscribe();
        plansState.change('id2_yearly').pipe(onErrorResumeNext()).subscribe();

        expect(plansState.snapshot.plans.values).toEqual([
            { isSelected: false, isYearlySelected: false, plan: oldPlans.plans[0] },
            { isSelected: false, isYearlySelected: true,  plan: oldPlans.plans[1] }
        ]);
        expect(plansState.snapshot.isOwner).toBeTruthy();
        expect(plansState.snapshot.version).toEqual(newVersion);
    });
});