namespace Domain.Tests;

internal class TestDataSource
{
    public const string CorrectPaymentJson = @"{
	""request"": {
		""id"": 27454821037510912,
		""document"": {
			""id"": 27454820926361856,
			""type"": ""INVOICE_PAYMENT""
		}
	},
	""debitPart"": {
		""agreementNumber"": ""RUS01"",
		""accountNumber"": ""30109810000000000001"",
		""amount"": 3442.79,
		""currency"": ""810"",
		""attributes"": {}
	},
	""creditPart"": {
		""agreementNumber"": ""RUS01"",
		""accountNumber"": ""30233810000000000001"",
		""amount"": 3442.79,
		""currency"": ""810"",
		""attributes"": {}
	},
	""details"": ""RASCHET"",
	""bankingDate"": ""2023-07-26"",
	""attributes"": {
		""attribute"": [
			{
				""code"": ""pack"",
				""attribute"": ""37""
			}
		]
	}
}";

    public const string PaymentJsonWithoutDebit = @"{
	""request"": {
		""id"": 27454821037510912,
		""document"": {
			""id"": 27454820926361856,
			""type"": ""INVOICE_PAYMENT""
		}
	},
	""creditPart"": {
		""agreementNumber"": ""RUS01"",
		""accountNumber"": ""30233810000000000001"",
		""amount"": 3442.79,
		""currency"": ""810"",
		""attributes"": {}
	},
	""details"": ""RASCHET"",
	""bankingDate"": ""2023-07-26"",
	""attributes"": {
		""attribute"": [
			{
				""code"": ""pack"",
				""attribute"": ""37""
			}
		]
	}
}";
}
