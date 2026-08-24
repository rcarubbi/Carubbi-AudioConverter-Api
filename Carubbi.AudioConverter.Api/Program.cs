using Carubbi.AudioConverter.Api.Converters;
using Carubbi.AudioConverter.Api.Utilities;
using Carubbi.AudioConverter.Api.Validators;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Carubbi Audio Converter API", Version = "v1" });
});

builder.Services.AddSingleton(new EnvironmentVariablesConfig());

builder.Services.AddTransient<IConverterSelector, ConverterSelector>();

builder.Services.AddTransient<IConverter, WavToMp3Converter>();
builder.Services.AddTransient<IConverter, Mp3ToWavConverter>();

builder.Services.AddTransient<IConverter, WavToOggConverter>();
builder.Services.AddTransient<IConverter, OggToWavConverter>();

builder.Services.AddTransient<IConverter, OggToMp3Converter>();
builder.Services.AddTransient<IConverter, Mp3ToOggConverter>();

builder.Services.AddTransient<IFileValidator, FileValidator>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();

app.UseSwagger();

app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Carubbi Audio Converter API V1");
});

app.UseAuthorization();

app.MapControllers();

app.Run();
