const handleNumberInEng = require('../Views/Shared/EngNumHelper');
const handleNumberInChi = require('../Views/Shared/ChiNumHelper');

test("Sanity check", () => {    
    expect(true).toBe(true);
});

test("handleNumberInEng", () => {    
    expect(handleNumberInEng('trillion and forty-two')).toBe(1_000_000_000_042);
    expect(handleNumberInEng('nineteen eighty four')).toBe(null);
    expect(handleNumberInEng('million million')).toBe(null);
    expect(handleNumberInEng('a million and a hundred thousand and a hundred')).toBe(1_100_100);
    expect(handleNumberInEng('FOUR hundred million and four hundred and fifty three thousand and sixty six')).toBe(400_453_066);
    expect(handleNumberInEng('  six thousand million five hundred zero five point seven eight  ')).toBe(6_000_000_505.78);
    expect(handleNumberInEng('  five TWO three four one zero naught nought siX  ')).toBe(523410006);
});

test("handleNumberInChi", () => {
    expect(handleNumberInChi('捌億陆千二百零四萬九千五佰叁拾五點零貳叁')).toBe(862049535.023);
    expect(handleNumberInChi('壹點貳叁肆零伍陸柒捌玖')).toBe(1.234056789);
    expect(handleNumberInChi('拾拾')).toBe(null);
    expect(handleNumberInChi('十拾')).toBe(null);
    expect(handleNumberInChi('十九八七六五四三二一零')).toBe(null);
    expect(handleNumberInChi('九八七六五四三二一零')).toBe(9876543210);
    expect(handleNumberInChi('十萬拾')).toBe(100_010);
    expect(handleNumberInChi('   兩百 貳十 点    五四叁弍壹   ')).toBe(220.54321);
    expect(handleNumberInChi('二點二点二')).toBe(null);
});
