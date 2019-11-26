using CryptoApp.Data;
using CryptoApp.Models;
using CryptoApp.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace CryptoApp.Controllers
{
    public class CryptoController : Controller
    {
        public readonly CryptoTransactionService _cryptoTransactionService;
        public readonly GetCryptoInfoService _getCryptoInfoService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        public CryptoController(ApplicationDbContext context, GetCryptoInfoService getCryptoInfoService, UserManager<ApplicationUser> userManager, CryptoTransactionService cryptoTransactionService)
        {
            _cryptoTransactionService = cryptoTransactionService;
            _getCryptoInfoService = getCryptoInfoService;
            _userManager = userManager;
            _context = context;

        }

        public async System.Threading.Tasks.Task<IActionResult> Index(string CryptoSymbol)
        {
         
            if (CryptoSymbol == null)
            {
                CryptoSymbol = "LTC";
            }
          
            CurrencyModel cryptoObject = _getCryptoInfoService.GetCryptoObject(CryptoSymbol).Result;

            return View(cryptoObject);
        }


        public async System.Threading.Tasks.Task<IActionResult> Wallet()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }
     

            var transactionModel = new TransactionModel
            {
                USD = user.USD,
                BTC = user.BTC,
                ETH = user.ETH,
                LTC = user.LTC, 
                DOGE = user.DOGE
                
            };


            WalletModel wallet = _cryptoTransactionService.GetWalletContents(transactionModel);

            return View(wallet);
        }



        public async Task<IActionResult> SellCrypto(string cryptoSymbol, decimal amountOnHand)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }
            if (cryptoSymbol == null)
            {
                cryptoSymbol = "LTC";
            }

            var transaction = new TransactionModel
            {
                USD = user.USD, 
                BTC = user.BTC, 
                ETH = user.ETH,
                LTC = user.LTC, 
                DOGE = user.DOGE,
                CryptoSymbol = cryptoSymbol,
            };

            SellViewModel sell = new SellViewModel
            {
                CryptoSymbol = transaction.CryptoSymbol,
                CryptoPrice = _getCryptoInfoService.GetCryptoPrice(transaction.CryptoSymbol),
                AmountToSell = transaction.AmountToBuyOrSell,
                AmountOnHand = transaction.AmountOnHand,
                USD = transaction.USD
            };


            return View(sell);
        }

        public async Task<IActionResult> BuyCrypto(string cryptoSymbol, decimal amountOnHand)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }
           

            var transaction = new TransactionModel
            {
                USD = user.USD, 
                BTC = user.BTC, 
                ETH = user.ETH, 
                LTC = user.LTC, 
                DOGE = user.DOGE,
                CryptoSymbol = cryptoSymbol,
            };

            SellViewModel sell = new SellViewModel
            {
                CryptoSymbol = transaction.CryptoSymbol,
                CryptoPrice = _getCryptoInfoService.GetCryptoPrice(transaction.CryptoSymbol),
                AmountToSell = transaction.AmountToBuyOrSell,
                AmountOnHand = transaction.AmountOnHand,
                USD = transaction.USD
            };


            return View(sell);
        }
        public async Task<IActionResult> ExecuteSale([Bind("USD,CryptoPrice,AmountToSell,CryptoSymbol")] SellViewModel inSell)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var transaction = new TransactionModel
            {
                USD = user.USD, 
                BTC = user.BTC, 
                ETH = user.ETH, 
                LTC = user.LTC, 
                DOGE = user.DOGE,
                CryptoSymbol = inSell.CryptoSymbol,
                AmountToBuyOrSell = inSell.AmountToSell
            };

            if (_cryptoTransactionService.CanSellCrypto(transaction))
            {
                transaction = _cryptoTransactionService.SellCryptoPerPiece(transaction);

                user.USD = transaction.USD;
                user.BTC = transaction.BTC;
                user.ETH = transaction.ETH;
                user.LTC = transaction.LTC;
                user.DOGE = transaction.DOGE;
                user.TotalAccountValue = transaction.TotalAccountValue;
                _context.SaveChanges();

                return RedirectToAction("Wallet", "Crypto");
            }
            else
            {
                return RedirectToAction("Wallet", "Crypto");
            }
        }
        public async Task<IActionResult> ExecuteBuy([Bind("USD,CryptoPrice,AmountToSell,CryptoSymbol")] SellViewModel inSell)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var transaction = new TransactionModel
            {
                USD = user.USD,
                BTC = user.BTC,
                ETH = user.ETH,
                LTC = user.LTC,
                DOGE = user.DOGE,
                CryptoSymbol = inSell.CryptoSymbol,
                AmountToBuyOrSell = inSell.AmountToSell
            };

            transaction = _cryptoTransactionService.BuyCryptoPerPiece(transaction);

            user.USD = transaction.USD;
            user.BTC = transaction.BTC;
            user.ETH = transaction.ETH;
            user.LTC = transaction.LTC;
            user.DOGE = transaction.DOGE;
            user.TotalAccountValue = transaction.TotalAccountValue;
            _context.SaveChanges();

            return RedirectToAction("Wallet", "Crypto");
        }

    }
}